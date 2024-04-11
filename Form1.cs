using System.Data;
using System.Data.SqlClient;

namespace WebAPICodeGenerator
{
    public partial class Form1 : Form
    {
        List<string> mylist =
                new(new string[] { "AD_", "AT_", "CSS_", "HRM_", "HU_","TR_",
                    "INS_", "PA_", "PT_", "RC_", "RPT_", "SE_", "SY_",
                    "SYS_", "THEME_", "TMP_","PER_" });
        List<string> SYSTEM_list =
                new(new string[] { "SYS_" });
        List<string> HU_list =
                new(new string[] { "PER_" });
        List<string> AT_list =
                new(new string[] { "AT_" });

        List<string> FIN_list =
                new(new string[] { "FIN_" });

        List<string> GOODS_list =
                new(new string[] { "GOODS_" });

        List<string>    CARD_list =
        new(new string[] { "CARD_", });

        List<string> ORD_list =
                new(new string[] { "ORD_" });
        List<string> LOC_list =
        new(new string[] { "LOC_" });

        List<string> ignoredlist =
                new(new string[] {
                                "_EFMigrationsHistory",
                                "AspNetRoleClaims",
                                "AspNetRoles",
                                "AspNetUserClaims",
                                "AspNetUserLogins",
                                "AspNetUserRoles",
                                "AspNetUserTokens",
                                "RPT_JOB_POS_HIS",
                                "PA_PAYROLLSHEET_SUM",
                                "AT_STRSQL"
                });

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string myPath = "";

            FolderBrowserDialog folderDlg = new();
            folderDlg.ShowNewFolderButton = false;
            DialogResult result = folderDlg.ShowDialog();


            if (result == DialogResult.OK)
            {
                myPath = folderDlg.SelectedPath;
            }

            if (myPath == "") return;

            using SqlConnection connection = new (
                       "Password=MatKhau@123;User ID=sa;Initial Catalog=GYM_SQL_DEV;Data Source=101.99.15.217,1433;TrustServerCertificate=True");

            SqlCommand command = new(
                @"SELECT TableName = name 
                        FROM sys.tables WHERE schema_id = 1
                        ORDER BY name
                ", connection);
            SqlDataAdapter da = new(command);
            DataSet ds = new();
            da.Fill(ds);
            DataTable dt = ds.Tables[0];
            var a = Helper.ConvertDataTable<MyTable>(dt);

            var filter = a.Where(x => mylist.IndexOf(x.TableName.GetTablePrefix()) >= 0)
                .Where(x => ignoredlist.IndexOf(x.TableName) < 0)
                .ToList();


            int index = 0;
            string ThreeDiditIndex;
            filter.ForEach(table =>
            {
                index++;
                if (index < 10)
                {
                    ThreeDiditIndex = "00" + index.ToString();
                }
                else if (index < 100)
                {
                    ThreeDiditIndex = "0" + index.ToString();
                }
                else
                {
                    ThreeDiditIndex = index.ToString();
                }
                var SNAKE_NAME = table.TableName;
                var PascalName = SNAKE_NAME.SnakeToCamelCase().CamelToPascalCase();
                Helper.GenerateControllerFolder(myPath, SNAKE_NAME);
                var interfaceFile = Helper.GenerateIRepositoryClass(SNAKE_NAME, PascalName);
                var repositoryFile = Helper.GenerateRepositoryClass(SNAKE_NAME, PascalName);

                string moduleName = "UNKNOWN";

                if (SNAKE_NAME.IndexOf("_") > 0)
                {
                    var i = SNAKE_NAME.IndexOf("_");
                    var prefix = SNAKE_NAME[..(i + 1)];
                    if (SYSTEM_list.IndexOf(prefix) == 0)
                    {
                        moduleName = "SYSTEM";
                    }
                    else if (HU_list.IndexOf(prefix) == 0)
                    {
                        moduleName = "PERSONAL";
                    }
                    else if (FIN_list.IndexOf(prefix) == 0)
                    {
                        moduleName = "FINANCE";
                    }
                    else if (LOC_list.IndexOf(prefix) == 0)
                    {
                        moduleName = "LOCKER";
                    }
                    else if (CARD_list.IndexOf(prefix) == 0)
                    {
                        moduleName = "CARD";
                    }
                    else if (ORD_list.IndexOf(prefix) == 0)
                    {
                        moduleName = "CARD";
                    }
                    else if (GOODS_list.IndexOf(prefix) == 0)
                    {
                        moduleName = "GOODS";
                    }
                    else 
                    {
                        moduleName = "OTHER";
                    }
                }
                else
                {
                    moduleName = "OTHER";
                }

                var controllerFile = Helper.GenerateControllerClass(SNAKE_NAME, PascalName, ThreeDiditIndex, moduleName);
                var dto = Helper.GenerateDtoClass(SNAKE_NAME, PascalName);

                var entity = Helper.GenerateEntityClass(SNAKE_NAME, PascalName);

                using (StreamWriter file = new(string.Format("{0}\\{1}\\I{1}Repository.cs", myPath, PascalName)))
                {
                    file.WriteLine(interfaceFile);
                }
                using (StreamWriter file = new(string.Format("{0}\\{1}\\{1}Repository.cs", myPath, PascalName)))
                {
                    file.WriteLine(repositoryFile);
                }
                using (StreamWriter file = new(string.Format("{0}\\{1}\\{1}Controller.cs", myPath, PascalName)))
                {
                    file.WriteLine(controllerFile);
                }


                using (StreamWriter file = new(string.Format("{0}\\{1}\\{2}.cs", myPath, PascalName, SNAKE_NAME)))
                {
                    file.WriteLine(entity);
                }
                using (StreamWriter file = new(string.Format("{0}\\{1}\\{1}DTO.cs", myPath, PascalName)))
                {
                    file.WriteLine(dto);
                }
            });

            MessageBox.Show("Done!", "Happy Coding!");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string myPath = "";

            FolderBrowserDialog folderDlg = new();
            folderDlg.ShowNewFolderButton = false;
            DialogResult result = folderDlg.ShowDialog();


            if (result == DialogResult.OK)
            {
                myPath = folderDlg.SelectedPath;
            }

            if (myPath == "") return;

            using SqlConnection connection = new SqlConnection(
                       "Password=MatKhau@123;User ID=sa;Initial Catalog=VEAM_SQL_DEV;Data Source=192.168.60.22,1433;TrustServerCertificate=True");

            List<string> mylist =
                new(new string[] { "AD_", "AT_", "CSS_", "HRM_", "HU_",
                    "INS_", "PA_", "PT_", "RC_", "RPT_", "SE_", "SY_",
                    "SYS_", "THEME_", "TMP_" });

            SqlCommand command = new(
                @"SELECT TableName = name 
                        FROM sys.tables WHERE schema_id = 1
                        ORDER BY name
                ", connection);
            SqlDataAdapter da = new(command);
            DataSet ds = new();
            da.Fill(ds);
            DataTable dt = ds.Tables[0];
            var a = Helper.ConvertDataTable<MyTable>(dt);

            var filter = a.Where(x => mylist.IndexOf(x.TableName.GetTablePrefix()) >= 0)
                .Where(x => ignoredlist.IndexOf(x.TableName) < 0)
                .ToList();

            Helper.GenerateFullDbContext(myPath, filter);

            MessageBox.Show("Done!", "Happy Coding!");
        }
    }

}