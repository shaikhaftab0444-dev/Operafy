<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Data.SqlClient" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/json";
        var result = new Dictionary<string, object>();
        List<string> allowedSubjects = new List<string>();

        if (Session["StudentID"] != null)
        {
            string studentID = Session["StudentID"].ToString();
            string connectionString = @"Data Source=A2NWPLSK14SQL-v01.shr.prod.iad2.secureserver.net;Initial Catalog=AITCenterDB;User ID=AITCenterUser;Password=Wainfo@123";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT DISTINCT CourseName FROM vw_StudentCourseDetails WHERE StudentID = @StudentID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentID", studentID);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    allowedSubjects.Add(dr["CourseName"].ToString().Trim().ToUpper());
                }
                dr.Close();
            }

            result["status"] = "success";
            result["allowed"] = allowedSubjects;
        }
        else
        {
            result["status"] = "unauthorized";
        }

        JavaScriptSerializer js = new JavaScriptSerializer();
        Response.Write(js.Serialize(result));
        Response.End();
    }
</script>
