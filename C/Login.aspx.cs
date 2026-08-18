using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        string email = txtEmailID.Text.Trim();
        string password = txtPassword.Text.Trim();
        string studentID = "";
        string firstName = "";
        string lastName = "";

        string connString = @"Data Source=A2NWPLSK14SQL-v01.shr.prod.iad2.secureserver.net;Initial Catalog=AITCenterDB;User ID=AITCenterUser;Password=Wainfo@123";

        using (SqlConnection conn = new SqlConnection(connString))
        {
            conn.Open();

            // ✅ 1. Check if email exists
            string emailQuery = "SELECT StudentID, Password, Status, FirstName, LastName, CourseName, RollNo, Passportsizephoto FROM registration WHERE EmailID=@EmailID";
            SqlCommand emailCmd = new SqlCommand(emailQuery, conn);
            emailCmd.Parameters.AddWithValue("@EmailID", email);

            SqlDataReader reader = emailCmd.ExecuteReader();

            if (!reader.HasRows)
            {
                // Email not found
                string script = "Swal.fire({icon:'error', title:'Oops!', text:'Email not found.'});";
                ClientScript.RegisterStartupScript(this.GetType(), "EmailError", script, true);
                reader.Close();
                return;
            }

            reader.Read();

            string dbPassword = reader["Password"].ToString();
            string status = reader["Status"].ToString();

            // ✅ 2. Check if student inactive
            if (status != "Active")
            {
                // Inactive account
                string script = "Swal.fire({icon:'warning', title:'Warning!', text:'Your account is inactive. Contact admin.'});";
                ClientScript.RegisterStartupScript(this.GetType(), "InactiveError", script, true);
                reader.Close();
                return;
            }

            // ✅ 3. Check password
            if (dbPassword != password)
            {
                // Password wrong
                string script = "Swal.fire({icon:'error', title:'Oops!', text:'Incorrect password.'});";
                ClientScript.RegisterStartupScript(this.GetType(), "PasswordError", script, true);
                reader.Close();
                return;
            }

            // ✅ 4. Successful login
            studentID = reader["StudentID"].ToString();
            string rollNo = reader["RollNo"].ToString(); // <-- Exam ExamRegistration Use 
            Session["CourseName"] = reader["CourseName"].ToString();// <-- Exam ExamRegistration Use 
            firstName = reader["FirstName"].ToString();
            lastName = reader["LastName"].ToString();
            string fullName = firstName + " " + lastName;

            // ✅ Store session values
            Session["PhotoFileName"] = reader["Passportsizephoto"].ToString();
            Session["StudentID"] = studentID;
            Session["RollNo"] = rollNo;        // <-- Exam ExamRegistration Use  
            Session["FirstName"] = firstName;
            Session["LastName"] = lastName;
            Session["StudentName"] = fullName;
            Session["User"] = "admin";
            Session["Login"] = studentID;
            Session["email"] = email;
            Session["reg"] = email;
            Session.Timeout = 90;  // Sets timeout to 90 minutes


            Session["UserID"] = studentID;
            Session["Username"] = fullName;
            Session["Role"] = "Student";
            Session["StudentIDAlias"] = "Student";

            // ✅ Cookie setup
            Response.Cookies["name"].Value = studentID;
            Response.Cookies["name"].Expires = DateTime.Now.AddMinutes(60);

            reader.Close();

            //  Cookie
            
            reader.Close();

            // ✅ Log login history
            string ipAddress = GetUserIP();
            string currentUrl = Request.Url.AbsoluteUri;
            string insertQuery = @"INSERT INTO LoginHistory 
                    (UserID, UserType, LoginTime, IPAddress, CurrentLocation)
                    VALUES (@UserID, @UserType, @LoginTime, @IPAddress, @CurrentLocation)";
            SqlCommand logCmd = new SqlCommand(insertQuery, conn);
            logCmd.Parameters.AddWithValue("@UserID", email);
            logCmd.Parameters.AddWithValue("@UserType", "Student");
            logCmd.Parameters.AddWithValue("@LoginTime", DateTime.Now);
            logCmd.Parameters.AddWithValue("@IPAddress", ipAddress);
            logCmd.Parameters.AddWithValue("@CurrentLocation", currentUrl);
            logCmd.ExecuteNonQuery();

            string loginScript = "Swal.fire({icon:'success', title:'Login Successful', text:'Welcome " + fullName + "'}).then(()=>{ window.location='../UserPanel/Default1.aspx'; });";
            ClientScript.RegisterStartupScript(this.GetType(), "LoginSuccess", loginScript, true);
// New code for redirect
 // ✅ Allowed Subjects from Approved Courses
                string courseQuery = @"
                    SELECT DISTINCT CourseName
                    FROM vw_StudentCourseDetails 
                    WHERE StudentID = @StudentID";

                SqlCommand courseCmd = new SqlCommand(courseQuery, conn);
                courseCmd.Parameters.AddWithValue("@StudentID", studentID);

                SqlDataReader dr = courseCmd.ExecuteReader();
                List<string> allowedSubjects = new List<string>();
                while (dr.Read())
                {
                    allowedSubjects.Add(dr["CourseName"].ToString().Trim().ToUpper());
                }
                Session["AllowedSubjects"] = allowedSubjects;
                dr.Close();

                // ✅ Redirect after login
                string returnUrl = Request.QueryString["returnUrl"];
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    if (!returnUrl.StartsWith("http"))
                        returnUrl = "https://aitaurangabad.com" + returnUrl;

                    Response.Redirect(returnUrl);
                }
                else
                {
                    Response.Redirect("https://aitaurangabad.com/UserPanel/CourseUpgrade.aspx");
                }


        }
    }


    protected void txtPassword_TextChanged(object sender, EventArgs e)
    {

    }
    protected void txtEmailID_TextChanged(object sender, EventArgs e)
    {

    }
    // ✅ Function to get user's actual IP Address
    private string GetUserIP()
    {
        string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
        if (string.IsNullOrEmpty(ip))
        {
            ip = Request.ServerVariables["REMOTE_ADDR"];
        }

        if (ip == "::1")
            ip = "127.0.0.1"; // Localhost fallback for testing

        return ip;
    }
}
   
