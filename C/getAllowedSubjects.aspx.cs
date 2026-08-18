using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class getAllowedSubjects : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        using (SqlCommand cmd = new SqlCommand(query, con))
        {
            cmd.Parameters.AddWithValue("@StudentID", studentID);
            SqlDataReader dr = cmd.ExecuteReader();

            List<string> allowedSubjects = new List<string>();

            while (dr.Read())
            {
                allowedSubjects.Add(dr["CourseName"].ToString().Trim().ToUpper());
            }

            Session["AllowedSubjects"] = allowedSubjects;
        }

    }
}