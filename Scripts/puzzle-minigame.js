// Minigame lật ô - Chủ đề mẹ & bé
const Minigame_MB_CARDS = [
  {icon:'🍼', name:'binhsua'},
  {icon:'👶', name:'embe'},
  {icon:'🧸', name:'thunhoybong'},
  {icon:'🚼', name:'golap'},
  {icon:'🍎', name:'tao'},
  {icon:'🩷', name:'tim'},
  {icon:'🧦', name:'tat'},
  {icon:'🪥', name:'banhchan'}
];
const minigameElem = {};
let minigameStartTs = 0;
const enumGameState = { idle:0, flipping:1, completed:2 };
let minigameState = { state:0, score:0, opened:[], done:[], board:[], steps:0, visible:[] };

function createMinigameModal() {
  if (document.getElementById('minigame-modal-bg')) return;
  const bg = document.createElement('div');
  bg.className = 'minigame-modal-bg';
  bg.id = 'minigame-modal-bg';
  bg.innerHTML = `<div class="minigame-modal-content">
    <button class="minigame-close-btn" onclick="closeMinigameModal()">&times;</button>
    <div class="minigame-header">
      <img src='/images/logo-bobby.png' alt="mom-baby-logo" />
      <h2>Minigame Lật Ô - Mẹ & Bé</h2>
      <div style='font-size:0.97em;line-height:1.4;color:#a96486;font-weight:600;'>Tìm cặp hình giống nhau để đạt điểm số và lọt top mẹ nhanh trí!</div>
    </div>
    <div class="minigame-scoreboard">Điểm số: <span id="minigame-score">0</span></div>
    <div class="minigame-board" id="minigame-board"></div>
    <div><button id="minigame-start-btn" class="minigame-start-btn">Chơi mới</button></div>
    <div class="minigame-leaderboard"><div class="minigame-leaderboard-title">Top điểm Mẹ & Bé</div><ul id="minigame-leaderboard-list"><li>Đang tải...</li></ul></div>
  </div>`;

  document.body.appendChild(bg);
  minigameElem.board = bg.querySelector('#minigame-board');
  minigameElem.scoreText = bg.querySelector('#minigame-score');
  bg.querySelector('#minigame-start-btn').onclick = startMinigameGame;
  loadLeaderboard();
  startMinigameGame();
}

function closeMinigameModal(){
  const bg = document.getElementById('minigame-modal-bg');
  if (bg) bg.remove();
}

function addMinigameButton(){
  if(document.getElementById('btn-open-minigame')) return;
  const btn = document.createElement('button');
  btn.id = 'btn-open-minigame';
  btn.className = 'minigame-btn';
  btn.innerHTML = '<img src="/images/logo-bobby.png" style="height:22px;width:22px;vertical-align:middle;margin-right:7px;">Minigame Mẹ & Bé';
  btn.style = 'position:fixed;bottom:90px;right:30px;z-index:9999;background:#fffbe5;color:#d63384;font-weight:700;font-size:1em;border:2px solid #fce4ec;border-radius:22px;padding:10px 20px 10px 13px;box-shadow:0 2px 14px #efa9cf3a;cursor:pointer;display:flex;align-items:center;gap:7px;';
  btn.onclick = createMinigameModal;
  document.body.appendChild(btn);
}

function shuffle(array){ let a=array.slice(); for(let i=a.length-1;i>0;i--){let j=Math.floor(Math.random()*(i+1));[a[i],a[j]]=[a[j],a[i]];} return a; }
function startMinigameGame() {
  minigameState.state = enumGameState.idle;
  minigameState.score = 0;
  minigameState.opened = [];
  minigameState.done = [];
  minigameState.visible = Array(16).fill(true);
  minigameState.steps = 0;
  minigameStartTs = Date.now();
  const fullset = shuffle([...Minigame_MB_CARDS, ...Minigame_MB_CARDS]);
  minigameState.board = fullset.map((c,i)=> ({...c, idx:i}) );
  minigameElem.scoreText.textContent = '0';
  renderMinigameBoard();
}
function renderMinigameBoard() {
  minigameElem.board.innerHTML = '';
  minigameState.board.forEach((card,idx)=>{
    let flipped = minigameState.done.includes(idx) || minigameState.opened.includes(idx);
    let disappeared = !minigameState.visible[idx];
    minigameElem.board.innerHTML += `<div class="minigame-card ${disappeared?'disappear':''} ${flipped?'flipped':''}" data-idx="${idx}" style="${disappeared?'opacity:0;pointer-events:none':''}" onclick="minigameFlipCard(${idx})">
      <div class="card-face card-front"></div>
      <div class="card-face card-back">${card.icon}</div>
    </div>`;
  });
}
window.minigameFlipCard = function(idx){
  if(minigameState.state===enumGameState.flipping || minigameState.done.includes(idx) || minigameState.opened.includes(idx) || !minigameState.visible[idx]) return;
  minigameState.opened.push(idx);
  renderMinigameBoard();
  if(minigameState.opened.length===2){
    minigameState.state = enumGameState.flipping;
    minigameState.steps++;
    setTimeout(()=>{
      const [i1,i2]=minigameState.opened;
      if(minigameState.board[i1].name===minigameState.board[i2].name){
        minigameState.done.push(i1,i2);
        minigameState.score++;
        minigameState.visible[i1] = false;
        minigameState.visible[i2] = false;
      }
      minigameState.opened=[];
      renderMinigameBoard();
      minigameElem.scoreText.textContent = minigameState.score;
      if(minigameState.done.length===minigameState.board.length){
        minigameState.state=enumGameState.completed;
        handleMinigameCompleted();
      }else{
        minigameState.state=enumGameState.idle;
      }
    },600);
  }
};
// Tính điểm theo yêu cầu: ít lượt lật + thời gian nhanh = điểm cao, có combo bonus
function handleMinigameCompleted(){
  let username = window.sessionStorage && window.sessionStorage.getItem('userName') || '';
  if(!username){
    const el = document.querySelector('[data-username],[data-name]');
    if(el) username = el.innerText||el.textContent; else username = 'Khách_'+Math.floor(Math.random()*9999);
    if(window.sessionStorage) window.sessionStorage.setItem('userName', username);
  }
  
  const completionTime = Math.floor((Date.now()-minigameStartTs)/1000);
  const minPossibleSteps = 8; // 8 cặp = tối thiểu 8 lượt lật
  
  // Điểm cơ bản: 1000 điểm tối đa
  let baseScore = 1000;
  
  // Trừ điểm theo số lượt lật thừa (mỗi lượt thừa trừ 50 điểm)
  let stepPenalty = Math.max(0, (minigameState.steps - minPossibleSteps) * 50);
  
  // Trừ điểm theo thời gian (mỗi giây trừ 10 điểm, sau 30s trừ nhiều hơn)
  let timePenalty = completionTime <= 30 ? completionTime * 10 : 300 + (completionTime - 30) * 20;
  
  // Thưởng combo: nếu hoàn thành với đúng 8 bước (perfect) thưởng 200 điểm
  let comboBonus = (minigameState.steps === minPossibleSteps) ? 200 : 0;
  
  // Thưởng tốc độ: nếu hoàn thành dưới 20s thưởng thêm
  let speedBonus = completionTime < 20 ? (20 - completionTime) * 15 : 0;
  
  let finalScore = Math.max(50, baseScore - stepPenalty - timePenalty + comboBonus + speedBonus);
  
  // Hiển thị thông báo hoàn thành với chi tiết điểm
  showCompletionMessage(finalScore, minigameState.steps, completionTime, comboBonus, speedBonus);
  
  saveScoreFirebase(username, Math.round(finalScore), minigameState.steps, completionTime);
}

// Hiển thị thông báo hoàn thành game với chi tiết điểm
function showCompletionMessage(score, steps, time, comboBonus, speedBonus) {
  const modal = document.querySelector('.minigame-modal-content');
  if (!modal) return;
  
  const messageDiv = document.createElement('div');
  messageDiv.className = 'completion-message';
  messageDiv.style = 'position:absolute;top:0;left:0;right:0;bottom:0;background:rgba(255,255,255,0.95);display:flex;flex-direction:column;justify-content:center;align-items:center;border-radius:18px;z-index:10;';
  
  let bonusText = '';
  if (comboBonus > 0) bonusText += `🎯 Perfect combo: +${comboBonus}đ<br>`;
  if (speedBonus > 0) bonusText += `⚡ Tốc độ: +${speedBonus}đ<br>`;
  
  messageDiv.innerHTML = `
    <div style="text-align:center;color:#d63384;">
      <h3 style="margin:0 0 15px;font-size:1.5em;">🎉 Hoàn thành!</h3>
      <div style="font-size:1.1em;margin-bottom:10px;">
        <strong style="font-size:1.3em;color:#e91e63;">${Math.round(score)} điểm</strong>
      </div>
      <div style="font-size:0.9em;color:#8e4a6b;line-height:1.4;">
        ${steps} bước • ${time} giây<br>
        ${bonusText}
      </div>
      <button onclick="this.parentElement.parentElement.remove()" style="margin-top:20px;background:#d63384;color:white;border:none;padding:8px 20px;border-radius:15px;cursor:pointer;font-weight:600;">Đóng</button>
    </div>
  `;
  
  modal.appendChild(messageDiv);
  
  // Tự động đóng sau 4 giây
  setTimeout(() => {
    if (messageDiv.parentElement) messageDiv.remove();
  }, 4000);
}
// Save với transaction: chỉ cập nhật nếu record mới vượt trội hơn
function saveScoreFirebase(username, score, steps, seconds){
  // Kiểm tra Firebase connection trước khi lưu
  if (!window.firebaseDb) {
    console.error('Firebase not connected');
    alert('Lỗi kết nối Firebase. Không thể lưu điểm.');
    return;
  }
  
  console.log('Saving score:', {username, score, steps, seconds});
  
  // Sử dụng key đơn giản hơn thay vì btoa
  const userKey = username.replace(/[^a-zA-Z0-9]/g, '_');
  const userRef = window.firebaseDb.ref('leaderboard/' + userKey);
  
  userRef.transaction(function(currentData){
    console.log('Current data:', currentData);
    if (currentData === null) {
      return {displayName: username, score: score, steps: steps, time: seconds, updated: Date.now()};
    }
    if (
      score > currentData.score ||
      (score === currentData.score && steps < currentData.steps) ||
      (score === currentData.score && steps === currentData.steps && seconds < currentData.time)
    ){
      return {displayName: username, score: score, steps: steps, time: seconds, updated: Date.now()};
    }
    return currentData;
  }, function(error, committed, snapshot){
    if (error) {
      console.error('Transaction failed: ', error);
      alert('Lỗi lưu điểm: ' + error.message);
    } else if (committed) {
      console.log('Score saved successfully!', snapshot.val());
    } else {
      console.log('Score not updated (current score is better)');
    }
    // Luôn cập nhật leaderboard sau khi transaction hoàn thành
    setTimeout(loadLeaderboard, 1000);
  });
}
function loadLeaderboard(){
  let leaderboardList = document.getElementById('minigame-leaderboard-list');
  if(!leaderboardList) return;
  leaderboardList.innerHTML = '<li>Đang tải...</li>';
  
  // Kiểm tra Firebase connection
  if (!window.firebaseDb) {
    leaderboardList.innerHTML = '<li>Lỗi kết nối Firebase</li>';
    return;
  }
  
  window.firebaseDb.ref('leaderboard').once('value')
    .then(snap => {
      let arr = [];
      snap.forEach(s => {
        const data = s.val();
        if (data && data.displayName && data.score !== undefined) {
          arr.push(data);
        }
      });
      
      arr.sort((a,b) => b.score - a.score || (a.steps - b.steps) || (a.time - b.time));
      leaderboardList.innerHTML = '';
      
      if (arr.length === 0) {
        leaderboardList.innerHTML = '<li>Chưa có mẹ nào!</li>';
      } else {
        arr.slice(0,7).forEach((p,i) => {
          leaderboardList.innerHTML += `<li style='padding:6px 0;'><span style="font-weight:700;width:18px;display:inline-block;color:${i==0?'#e91e63':'#8e4a6b'}">${i+1}.</span> <span style="font-weight:600;color:#d63384">${p.displayName||'Mẹ'}</span> <span style='color:#686;'>${p.score}đ</span> <small style='color:#999'>&bull; ${p.steps} bước &bull; ${p.time}s</small></li>`;
        });
      }
    })
    .catch(error => {
      console.error('Error loading leaderboard:', error);
      leaderboardList.innerHTML = '<li>Lỗi tải bảng xếp hạng</li>';
    });
}
if (document.readyState==='loading') document.addEventListener('DOMContentLoaded',addMinigameButton); else addMinigameButton();
