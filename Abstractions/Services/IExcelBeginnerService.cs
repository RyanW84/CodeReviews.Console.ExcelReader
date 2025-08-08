using System.Data;
public interface IExcelBeginnerService
   {
       Task<DataTable> ReadFromExcelAsync();
       DataTable ReadFromExcel();
   }