using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Syncfusion.JavaScript;
using Syncfusion.JavaScript.DataSources;
using Syncfusion.JavaScript.Mobile;

namespace FarmerBrothers.Controllers
{
    public class DropdownController : Controller
    {
        //


        public static List<Orders> orders = new List<Orders>();
        //public static List<Employees> emps = new List<Employees>();
        public ActionResult Index()
        {
            var DataSource = GetEmployeeView();//new NorthwindDataContext().EmployeeViews.Take(9).ToList();
            ViewBag.datasource = DataSource;
            //emps = GetEmployees();
            //BindDataSource();
            ViewBag.dataSource2 = GetOrders();
            return View(orders);
        }
        //public void BindDataSource()
        //{
        //    if (order != null)
        //    {
        //        for (var i = 0; i < 20000; i++)
        //            order.Add(new Orders(i, "SBI" + i));
        //    }
        ////}
        public ActionResult dropDown(Syncfusion.JavaScript.DataManager dm)
        {
            return View();

        }

        public static List<Orders> GetOrders()
        {
            //List<Orders> orders = new List<Orders>();
            for (int i = 1; i < 10000; i++)
            {
                Orders order = new Orders();
                order.EmployeeID = i;
                order.FirstName = "Lucky" + i;
                //order.Title = "TL" + i;
                //order.City = "HYD" + i;
                //order.Country = "IND" + i;
                orders.Add(order);
            }
            return orders;
        }

        //public static List<Employees> GetEmployees()
        //{
           
        //    for (int i = 1; i < 10; i++)
        //    {
        //        Employees emp = new Employees();
        //        emp.EmployeeID = i;
        //        emp.FirstName = "Lucky" + i;
        //        emps.Add(emp);
        //    }
        //    return emps;
        //}

        public static List<EmployeeView> GetEmployeeView()
        {
            List<EmployeeView> emps = new List<EmployeeView>();
            for (int i = 1; i < 10; i++)
            {
                EmployeeView emp = new EmployeeView();
                emp.EmployeeID = i;
                emp.FirstName = "Lucky" + i;
                emp.Title = "TL" + i;
                emp.City = "HYD" + i;
                emp.Country = "IND" + i;
                emps.Add(emp);
            }
            return emps;
        }
    }
    //public class Orders
    //{
    //    public int EmployeeID { get; set; }
    //    public string FirstName { get; set; }
    //    public string Title { get; set; }
    //    public string City { get; set; }
    //    public string Country { get; set; }
    //}

    //[global::System.Data.Linq.Mapping.TableAttribute(Name = "dbo.EmployeeView")]
    public partial class EmployeeView
    {

        private int _EmployeeID;

        private string _LastName;

        private string _FirstName;

        private string _Title;

        private string _TitleOfCourtesy;

        private System.Nullable<System.DateTime> _BirthDate;

        private System.Nullable<System.DateTime> _HireDate;

        private string _Address;

        private string _City;

        private string _Region;

        private string _PostalCode;

        private string _Country;

        private string _HomePhone;

        private string _Extension;

        //private System.Data.Linq.Binary _Photo;

        private string _Notes;

        private System.Nullable<int> _ReportsTo;

        private string _PhotoPath;

        public EmployeeView()
        {
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_EmployeeID", AutoSync = AutoSync.Always, DbType = "Int NOT NULL IDENTITY", IsDbGenerated = true)]
        public int EmployeeID
        {
            get
            {
                return this._EmployeeID;
            }
            set
            {
                if ((this._EmployeeID != value))
                {
                    this._EmployeeID = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_LastName", DbType = "NVarChar(20) NOT NULL", CanBeNull = false)]
        public string LastName
        {
            get
            {
                return this._LastName;
            }
            set
            {
                if ((this._LastName != value))
                {
                    this._LastName = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_FirstName", DbType = "NVarChar(10) NOT NULL", CanBeNull = false)]
        public string FirstName
        {
            get
            {
                return this._FirstName;
            }
            set
            {
                if ((this._FirstName != value))
                {
                    this._FirstName = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Title", DbType = "NVarChar(30)")]
        public string Title
        {
            get
            {
                return this._Title;
            }
            set
            {
                if ((this._Title != value))
                {
                    this._Title = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_TitleOfCourtesy", DbType = "NVarChar(25)")]
        public string TitleOfCourtesy
        {
            get
            {
                return this._TitleOfCourtesy;
            }
            set
            {
                if ((this._TitleOfCourtesy != value))
                {
                    this._TitleOfCourtesy = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_BirthDate", DbType = "DateTime")]
        public System.Nullable<System.DateTime> BirthDate
        {
            get
            {
                return this._BirthDate;
            }
            set
            {
                if ((this._BirthDate != value))
                {
                    this._BirthDate = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_HireDate", DbType = "DateTime")]
        public System.Nullable<System.DateTime> HireDate
        {
            get
            {
                return this._HireDate;
            }
            set
            {
                if ((this._HireDate != value))
                {
                    this._HireDate = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Address", DbType = "NVarChar(60)")]
        public string Address
        {
            get
            {
                return this._Address;
            }
            set
            {
                if ((this._Address != value))
                {
                    this._Address = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_City", DbType = "NVarChar(15)")]
        public string City
        {
            get
            {
                return this._City;
            }
            set
            {
                if ((this._City != value))
                {
                    this._City = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Region", DbType = "NVarChar(15)")]
        public string Region
        {
            get
            {
                return this._Region;
            }
            set
            {
                if ((this._Region != value))
                {
                    this._Region = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_PostalCode", DbType = "NVarChar(10)")]
        public string PostalCode
        {
            get
            {
                return this._PostalCode;
            }
            set
            {
                if ((this._PostalCode != value))
                {
                    this._PostalCode = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Country", DbType = "NVarChar(15)")]
        public string Country
        {
            get
            {
                return this._Country;
            }
            set
            {
                if ((this._Country != value))
                {
                    this._Country = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_HomePhone", DbType = "NVarChar(24)")]
        public string HomePhone
        {
            get
            {
                return this._HomePhone;
            }
            set
            {
                if ((this._HomePhone != value))
                {
                    this._HomePhone = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Extension", DbType = "NVarChar(4)")]
        public string Extension
        {
            get
            {
                return this._Extension;
            }
            set
            {
                if ((this._Extension != value))
                {
                    this._Extension = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Photo", DbType = "Image", UpdateCheck = UpdateCheck.Never)]
        //public System.Data.Linq.Binary Photo
        //{
        //    get
        //    {
        //        return this._Photo;
        //    }
        //    set
        //    {
        //        if ((this._Photo != value))
        //        {
        //            this._Photo = value;
        //        }
        //    }
        //}

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Notes", DbType = "NText", UpdateCheck = UpdateCheck.Never)]
        public string Notes
        {
            get
            {
                return this._Notes;
            }
            set
            {
                if ((this._Notes != value))
                {
                    this._Notes = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_ReportsTo", DbType = "Int")]
        public System.Nullable<int> ReportsTo
        {
            get
            {
                return this._ReportsTo;
            }
            set
            {
                if ((this._ReportsTo != value))
                {
                    this._ReportsTo = value;
                }
            }
        }

        //[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_PhotoPath", DbType = "NVarChar(255)")]
        public string PhotoPath
        {
            get
            {
                return this._PhotoPath;
            }
            set
            {
                if ((this._PhotoPath != value))
                {
                    this._PhotoPath = value;
                }
            }
        }
    }

    public class Orders
    {
        public Orders()
        {

        }
        public Orders(int EmployeeID, string FirstName)
        {
            this.EmployeeID = EmployeeID;
            this.FirstName = FirstName;


        }

        public int EmployeeID { get; set; }
        public string FirstName { get; set; }


    }
}