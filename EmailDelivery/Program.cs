using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;

namespace DrawingDelivery
{
    class Program
    {
        //Find rows 
        static void CheckQueue(SqlConnection cn)
        {
            //Count Rows
            string sqlStr = @"select COUNT(dq.ID)
                            from SM_CPQ_Model.dbo.smEmailQueue dq
                            where [Status] = 0";

            SqlCommand cmdCount = new SqlCommand(sqlStr, cn);
            int cnt = (int)cmdCount.ExecuteScalar();

            Console.WriteLine(cnt + " emails in queue");

            //Loop Rows
            while (cnt > 0)
            {

                sqlStr = "select MIN(ID) from SM_CPQ_Model.dbo.smEmailQueue where status = 0";
                SqlCommand cmd = new SqlCommand(sqlStr, cn);
                int id = (int)cmd.ExecuteScalar();
                SendEmail(cn, id);
                cmd = null;
                //Update Line
                sqlStr = @"update SM_CPQ_Model.dbo.smEmailQueue
                        set [Status] = 1
                        ,[DateSent] = CURRENT_TIMESTAMP
                        where id = @ID";
                SqlCommand cmdUpdate = new SqlCommand(sqlStr, cn);
                cmdUpdate.Parameters.AddWithValue("@ID", id);
                cmdUpdate.ExecuteNonQuery();
                cnt = 0;
            }
        }

        //Deliver Email
        static void SendEmail(SqlConnection cn, int id)
        {

            string sqlStr = @"select dq.FilePath, dq.DetailID, dq.[Label] , dq.[To], dq.Body    
                            from SM_CPQ_Model.dbo.smEmailQueue dq
                            where dq.[ID] = @ID";
            SqlCommand cmd = new SqlCommand(sqlStr, cn);
            cmd.Parameters.AddWithValue("@ID", id);
            SqlDataReader r = cmd.ExecuteReader();
            while (r.Read())
            {
                //Get Message Values
                string Label = r["Label"].ToString();
                string server = "mail.smtp2go.com";
                string to = r["To"].ToString();
                string from = "cpq@sunmountaindoor.com";

                string subject = Label;
                string body = r["Body"].ToString();
                //string file = @"\\Ranger\" + FilePath + @"Output.dwg";

                Console.WriteLine(to);
                Console.WriteLine(from);
                Console.WriteLine(subject);
                Console.WriteLine(body);
                //Console.WriteLine(file);
                //Console.WriteLine("Press any key to continue");
                //Console.ReadLine();

                //Send message
                //Attachment a = new Attachment(file);
                //a.Name = Label + ".dwg";
                //a.Name = OrderNo + "_" + LineNo + ".dwg";
                MailMessage message = new MailMessage(from, to);
                message.Subject = subject;
                message.Body = body;
                //message.Attachments.Add(a);
                SmtpClient client = new SmtpClient(server);
                //Plaintext credentials, switch to environment variables or a settings file as soon as feasible
                var basicCredential = new NetworkCredential("SMD_smtp", "YupQ4SkmjKFLaiAC");
                client.Credentials = basicCredential;
                //client.UseDefaultCredentials = true;
                client.Send(message);
                message.Dispose();
            }
            r.Close();
        }

        //Sent Email on Startup
        static void SendStartEmail()
        {
            string server = "mail.smtp2go.com";
            string to = "jantonacci@sunmountaindoor.com";
            string from = "cpq@sunmountaindoor.com";

            string subject = "Start Email Deliver";
            string body = "hello joe...";

            Console.WriteLine(to);
            Console.WriteLine(from);
            Console.WriteLine(subject);
            Console.WriteLine(body);

            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = body;
            SmtpClient client = new SmtpClient(server);
            //Plaintext credentials, switch to environment variables or a settings file as soon as feasible
            var basicCredential = new NetworkCredential("SMD_smtp", "YupQ4SkmjKFLaiAC");
            client.Credentials = basicCredential;
            client.Send(message);
            message.Dispose();
        }

        //Main
        static void Main(string[] args)
        {
            SendStartEmail();

            //Set connection string; open connection
            string cnStr = "Data Source=Ranger;Initial Catalog=SM_SalesPortal;Trusted_Connection=true;";
            SqlConnection cn = new SqlConnection(cnStr);
            cn.Open();

            //Loop 
            int i = 1;
            //Time in seconds
            int t = 1;
            //Loop
            while (i != 0)
            {
                //Console.WriteLine(i*1);
                System.Threading.Thread.Sleep(t*5000);
                CheckQueue(cn);
            }

            //Close Connection
            cn.Close();
        }
    }
}
