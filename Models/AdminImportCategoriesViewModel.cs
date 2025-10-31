using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace B_M.Models
{
    public class AdminImportCategoriesViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn file Excel")]
        public HttpPostedFileBase ExcelFile { get; set; }

        public bool SkipDuplicateNames { get; set; } = true;
    }

    public class AdminImportCategoriesResultViewModel
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int SkippedCount { get; set; }
        public List<ImportCategoryError> Errors { get; set; } = new List<ImportCategoryError>();
        public List<ImportCategorySuccess> SuccessCategories { get; set; } = new List<ImportCategorySuccess>();
        public DateTime ImportTime { get; set; } = DateTime.Now;
        public string FileName { get; set; }
    }

    public class ImportCategoryError
    {
        public int RowNumber { get; set; }
        public string CategoryName { get; set; }
        public string ErrorMessage { get; set; }
        public string FieldName { get; set; }
    }

    public class ImportCategorySuccess
    {
        public int RowNumber { get; set; }
        public string CategoryName { get; set; }
        public string ParentCategoryName { get; set; }
        public bool IsB2CEnabled { get; set; }
        public bool IsC2CEnabled { get; set; }
    }
}


