using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dashboard.ViewModel.ConfigDialog
{
    public class LoginViewModel : NotifyPropertyChanged
    {
        public LoginViewModel()
        {
            Load();
        }

        private void Load()
        {
            _acc = new List<AccountInfo>();
        }
        public string UserName
        {
            get { return _username; }
            set
            {
                _username = value;
                RaisePropertyChanged("UserName");
            }
        }
        private string _username;

        public IEnumerable<AccountInfo> Account
        {
            get { return _acc; }
            set
            {
                _acc = value.ToList();
                UserName = _acc.FirstOrDefault().Name;
            }
        }
        private List<AccountInfo> _acc; //使用可列舉物件是因為一組 Username+Password 可能會有多個帳號權限(e.g power user + sa + ...)


        private void Login(Object obj)
        {
            // 在此考慮安全性所以獨立處理登入動作
            System.Windows.Controls.PasswordBox pwBpx = obj as System.Windows.Controls.PasswordBox;
            if (obj != null)
            {
                string psw = pwBpx.Password;
                _acc.Clear();
                using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString()))
                {

                    using (SqlCommand sqlcmnd = new SqlCommand())
                    {
                        sqlcmnd.Connection = conn;
                        StringBuilder query = new StringBuilder();
                        query.AppendLine("SELECT COUNT(*) FROM (");
                        query.AppendLine("SELECT a.USERNAME, a.[PASSWORD], a.SITE_ID, c.NAME, c.[LEVEL] FROM USERS a");
                        query.AppendLine("JOIN USER_ACC_INFO b ON a.USER_INDEX = b.USER_INDEX");
                        query.AppendLine("JOIN ACCOUNTS c ON b.ACC_INDEX = c.ACC_INDEX");
                        query.AppendLine(") AS T");
                        query.AppendLine("WHERE [USERNAME]=@username AND [PASSWORD]=@pw");
                        sqlcmnd.CommandText = query.ToString();
                        sqlcmnd.Parameters.AddWithValue("username", UserName);
                        sqlcmnd.Parameters.AddWithValue("pw", psw);

                        try
                        {
                            conn.Open();
                            int adminExist = (int)sqlcmnd.ExecuteScalar();
                            if (adminExist > 0)
                            {
                                //確認該使用者的帳號資訊
                                query.Clear();
                                query.AppendLine("SELECT a.SITE_ID, a.USERNAME, a.FIRSTNAME, a.LASTNAME, c.NAME, c.[LEVEL] FROM USERS a");
                                query.AppendLine("JOIN USER_ACC_INFO b ON a.USER_INDEX = b.USER_INDEX");
                                query.AppendLine("JOIN ACCOUNTS c ON b.ACC_INDEX = c.ACC_INDEX");
                                query.AppendLine("WHERE a.[USERNAME]=@username AND a.[PASSWORD]=@pw");
                                sqlcmnd.CommandText = query.ToString();
                                SqlDataAdapter da = new SqlDataAdapter(sqlcmnd);
                                System.Data.DataTable dt = new System.Data.DataTable();
                                da.Fill(dt);

                                foreach (System.Data.DataRow dr in dt.Rows)
                                {
                                    _acc.Add(new AccountInfo
                                    {
                                        Name = dr["USERNAME"].ToString(),
                                        FirstName = dr["FIRSTNAME"].ToString(),
                                        LastName = dr["LASTNAME"].ToString(),
                                        Role = dr["Name"].ToString(),
                                        Site = dr["SITE_ID"].ToString(),
                                        Level = dr["LEVEL"].ToString()
                                    });
                                }
                                conn.Close();
                                da.Dispose();
                                CloseAction();
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("登入帳號或密碼錯誤", "",
                                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(string.Format("於連接資料伺服器時發生異常\r\n{0}\r\n請與管理者聯繫。", ex.Message), "",
                                   System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }

                    }
                }


            }
        }
        private void ChangePassWord(Object obj)
        {
            // 在此考慮安全性所以獨立處理登入動作
            // 輸入的物件應該是一個由PasswordBox組成的可簡單列舉
            var values = obj as IEnumerable<Object>;
            if (values != null)
            {
                string oldPsw = "";
                string newPsw = "";
                string confirmNewPsw = "";
                //取得每個 PasswordBox 的密碼
                foreach (System.Windows.Controls.PasswordBox item in values)
                {
                    switch (item.Name)
                    {
                        case "OldPsw":
                            oldPsw = item.Password;
                            break;
                        case "NewPsw":
                            newPsw = item.Password;
                            break;
                        case "ConfirmNewPsw":
                            confirmNewPsw = item.Password;
                            break;
                        default:
                            break;
                    }
                }

                if (newPsw.Length < 6)
                {
                    System.Windows.MessageBox.Show(
                        "密碼長度不可少於6，請重新輸入。", "",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (newPsw != confirmNewPsw)
                {
                    System.Windows.MessageBox.Show(
                        "新密碼與確認新密碼內容不符，請重新確認。", "",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString()))
                {

                    using (SqlCommand sqlcmnd = new SqlCommand())
                    {
                        sqlcmnd.Connection = conn;
                        StringBuilder query = new StringBuilder();
                        query.AppendLine("SELECT COUNT(*) FROM (");
                        query.AppendLine("SELECT a.USERNAME, a.[PASSWORD], a.SITE_ID, c.NAME, c.[LEVEL] FROM USERS a");
                        query.AppendLine("JOIN USER_ACC_INFO b ON a.USER_INDEX = b.USER_INDEX");
                        query.AppendLine("JOIN ACCOUNTS c ON b.ACC_INDEX = c.ACC_INDEX");
                        query.AppendLine(") AS T");
                        query.AppendLine("WHERE [USERNAME]=@username AND [PASSWORD]=@pw");
                        sqlcmnd.CommandText = query.ToString();
                        sqlcmnd.Parameters.AddWithValue("username", UserName);
                        sqlcmnd.Parameters.AddWithValue("pw", oldPsw);

                        try
                        {
                            conn.Open();
                            int adminExist = (int)sqlcmnd.ExecuteScalar();
                            if (adminExist > 0) //密碼與該帳號吻合
                            {
                                //更新使用者的密碼
                                query.Clear();
                                query.AppendLine("UPDATE USERS");
                                query.AppendLine("SET [PASSWORD]=@pw");
                                query.AppendLine("WHERE [USERNAME]=@username");
                                sqlcmnd.CommandText = query.ToString();
                                sqlcmnd.Parameters["pw"].Value = newPsw;
                                if (sqlcmnd.ExecuteNonQuery() == 0) throw new Exception("無法更新密碼");
                                conn.Close();
                                System.Windows.MessageBox.Show("更新密碼完成", "",
                                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                                CloseAction();
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("登入帳號或密碼錯誤", "",
                                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(string.Format("於連接資料伺服器時發生異常\r\n{0}\r\n請與管理者聯繫。", ex.Message), "",
                                   System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }

                    }


                }
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                return new Command.RelayCommand(Login);
            }
        }
        public ICommand ChangePasswordCommand
        {
            get
            {
                return new Command.RelayCommand(ChangePassWord);
            }
        }




    }
    public struct AccountInfo
    {
        public string Site;
        public string Name;
        public string FirstName;
        public string LastName;
        public string Role;
        public string Level;
    }
}
