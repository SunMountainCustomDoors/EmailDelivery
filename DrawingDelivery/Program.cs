using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Net.Mail;

namespace DrawingDelivery
{
    class Program
    {
        //
        static void CheckQueue(SqlConnection cn)
        {
            //Count Rows
            //string sqlStr = @"select COUNT(dq.ID)
            //                from SM_CPQ_Model.dbo.smDrawingQueue dq
            //                left join SM_SalesPortal.dbo.OrderDetails od
            //                on dq.DetailID = od.ConfigurationID
            //                where dq.status = 0
            //               and od.OrderNo is not null";

            string sqlStr = @"select COUNT(dq.ID)
                            from SM_CPQ_Model.dbo.smDrawingQueue dq
                            where [Status] = 0";

            SqlCommand cmdCount = new SqlCommand(sqlStr, cn);
            int cnt = (int)cmdCount.ExecuteScalar();

            //Loop Rows
            while (cnt > 0)
            {

                sqlStr = "select MIN(ID) from SM_CPQ_Model.dbo.smDrawingQueue where status = 0";
                SqlCommand cmd = new SqlCommand(sqlStr, cn);
                int id = (int)cmd.ExecuteScalar();
                SendEmail(cn, id);
                cmd = null;
                //Update Line
                sqlStr = @"update SM_CPQ_Model.dbo.smDrawingQueue
                        set [Status] = 1
                        ,[DateSent] = CURRENT_TIMESTAMP
                        where id = @ID";
                SqlCommand cmdUpdate = new SqlCommand(sqlStr, cn);
                cmdUpdate.Parameters.AddWithValue("@ID", id);
                cmdUpdate.ExecuteNonQuery();
                cnt = 0;
            }
        }


        static void SendEmail(SqlConnection cn, int id)
        {
            //string sqlStr = @"select od.OrderNo, od.LockUser, dq.FilePath, dq.DetailID, od.[LineNo] ,od.[Label] , dq.[To]    
            //                from SM_CPQ_Model.dbo.smDrawingQueue dq
            //                left join SM_SalesPortal.dbo.OrderDetails od
            //                on dq.DetailID = od.ConfigurationID
            //                where dq.[ID] = @ID";
            string sqlStr = @"select dq.FilePath, dq.DetailID, dq.[Label] , dq.[To]    
                            from SM_CPQ_Model.dbo.smDrawingQueue dq
                            where dq.[ID] = @ID";
            SqlCommand cmd = new SqlCommand(sqlStr, cn);
            cmd.Parameters.AddWithValue("@ID", id);
            SqlDataReader r = cmd.ExecuteReader();
            while (r.Read())
            {
                //Get Message Values

                string Label = r["Label"].ToString();
                //string LockUser = r["LockUser"].ToString();
                string FilePath = r["FilePath"].ToString();
                string DetailID = r["DetailID"].ToString();
                string server = "192.168.1.40";
                string to = r["To"].ToString();
                string from = "cpq@sunmountaindoor.com";

                string subject = Label;
                string body = "";
                string file = @"\\Ranger\" + FilePath + @"Output.dwg";
                //string OrderNo = r["OrderNo"].ToString();
                //string LineNo = r["LineNo"].ToString();
                //string to = LockUser + "@sunmountaindoor.com";
                //string subject = OrderNo + ", Line " + LineNo;
                //string subject = OrderNo + ", " + Label;

                Console.WriteLine(to);
                Console.WriteLine(from);
                Console.WriteLine(subject);
                Console.WriteLine(body);
                Console.WriteLine(file);
                //Console.WriteLine("Press any key to continue");
                //Console.ReadLine();

                //Send message
                Attachment a = new Attachment(file);
                a.Name = Label + ".dwg";
                //a.Name = OrderNo + "_" + LineNo + ".dwg";
                MailMessage message = new MailMessage(from, to);
                message.Subject = subject;
                message.Body = body;
                message.Attachments.Add(a);
                SmtpClient client = new SmtpClient(server);
                //client.UseDefaultCredentials = true;
                client.Send(message);
            }
            r.Close();
        }

        //Main
        static void Main(string[] args)
        {
            //Set connection string; open connection
            string cnStr = "Data Source=Ranger;Initial Catalog=SM_SalesPortal;Trusted_Connection=true;";
            SqlConnection cn = new SqlConnection(cnStr);
            cn.Open();

            //Loop 
            int i = 0;
            //Time in seconds
            int t = 1;
            //Loop 2880 times (8 hrs) * 3
            while (i <= 3*2880)
            {
                Console.WriteLine(i*1);
                System.Threading.Thread.Sleep(t*1000);
                //QueueOrders(cn);
                //ProcessQueue(cn);
                //Console.WriteLine("Press any key");
                //Console.ReadLine();
                CheckQueue(cn);
                i = i + 1;
            }

            //Close Connection
            cn.Close();
        }
    }
}
