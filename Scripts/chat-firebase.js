// Basic real-time chat JS using Firebase
function getRoomId(user1, user2) {
    user1 = parseInt(user1); user2 = parseInt(user2);
    return user1 < user2 ? `room_${user1}_${user2}` : `room_${user2}_${user1}`;
}

// Không khai báo ROOM_ID hay db ngoài hàm! (để tránh lỗi khi biến chưa tồn tại)
function getDb() {
    if (!firebase.apps.length) throw new Error("Firebase app chưa khởi tạo!");
    return firebase.database();
}

function getRoomVars() {
    if (typeof CURRENT_USER_ID === 'undefined') throw new Error('CURRENT_USER_ID not set');
    if (typeof OTHER_USER_ID === 'undefined') throw new Error('OTHER_USER_ID not set');
    return {
        db: getDb(),
        ROOM_ID: getRoomId(CURRENT_USER_ID, OTHER_USER_ID)
    };
}

// Send a text message
function sendMessage(msg) {
    const { db, ROOM_ID } = getRoomVars();
    db.ref(`chat/${ROOM_ID}/messages`).push({
        senderId: CURRENT_USER_ID,
        senderName: CURRENT_USER_NAME,
        avatar: CURRENT_USER_AVATAR,
        text: msg,
        timestamp: Date.now(),
        type: 'text'
    });
}

// Listen for new messages
function listenMessages(onMsg) {
    const { db, ROOM_ID } = getRoomVars();
    db.ref(`chat/${ROOM_ID}/messages`).off();
    db.ref(`chat/${ROOM_ID}/messages`).on('child_added', snap => {
        onMsg(snap.val(), snap.key);
    });
}

// Send online status
function setOnlineStatus(isOnline) {
    const { db, ROOM_ID } = getRoomVars();
    db.ref(`chat/${ROOM_ID}/onlineStatus/${CURRENT_USER_ID}`).set(isOnline);
}

// Listen online status
function listenOnlineStatus(onStatus) {
    const { db, ROOM_ID } = getRoomVars();
    db.ref(`chat/${ROOM_ID}/onlineStatus`).off();
    db.ref(`chat/${ROOM_ID}/onlineStatus`).on('value', snap => {
        onStatus(snap.val() || {});
    });
}

// Upload image and send message
function sendImage(file, onUploaded) {
    const { db, ROOM_ID } = getRoomVars();
    const storageRef = firebase.storage().ref(`chat-images/${ROOM_ID}/${Date.now()}_${file.name}`);
    storageRef.put(file).then(snapshot => snapshot.ref.getDownloadURL())
        .then(downloadURL => {
            db.ref(`chat/${ROOM_ID}/messages`).push({
                senderId: CURRENT_USER_ID,
                senderName: CURRENT_USER_NAME,
                avatar: CURRENT_USER_AVATAR,
                imageUrl: downloadURL,
                timestamp: Date.now(),
                type: 'image'
            });
            if(onUploaded) onUploaded(downloadURL);
        });
}

// Khi mở giao diện chat, nhớ set đúng window.OTHER_USER_ID trước mọi truy cập hàm trên!
// Khi DOM/Window sẵn sàng, set online (bọc xử lý lỗi context!)
document.addEventListener('DOMContentLoaded', () => {
    try { setOnlineStatus(true); } catch(e) {}
});
window.addEventListener('beforeunload', () => {
    try { setOnlineStatus(false); } catch(e) {}
});
// ... UI binding code sẽ thêm sau ...