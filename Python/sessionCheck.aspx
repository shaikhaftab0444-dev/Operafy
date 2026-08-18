<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        var result = new Dictionary<string, object>();

        string user = Convert.ToString(Session["User"]);
        string login = Convert.ToString(Session["Login"]);
        string studentID = Convert.ToString(Session["StudentID"]);

        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(login) &&
            (user == "student" || user == "admin"))
        {
            result["status"] = "authorized";
            result["studentID"] = studentID;
            result["allowed"] = "yes";  // Optional field
        }
        else
        {
            result["status"] = "unauthorized";
        }

        Response.ContentType = "application/json";
        JavaScriptSerializer js = new JavaScriptSerializer();
        Response.Write(js.Serialize(result));
        Response.End();
    }
</script>
