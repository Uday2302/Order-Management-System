using Order_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dapper;
using System.IO;
using System.Web;
using System.Net.Mail;

namespace Order_Management_System.Controllers
{
   [RoutePrefix("OrderManagement")]
    public class OrderManagementController : ApiController
    {
        [Route("GetOrderDetails")]
        [HttpGet]
        public List<OrderViewModel> GetOrderDetails(int? orderId, bool isAdmin)
        {
            List<OrderViewModel> OrderList;
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["DapperConnectionString"].ConnectionString))
            {

                OrderList = db.Query<OrderViewModel>("USP_Get_OrderDetails", new { OrderId = orderId, IsAdmin = isAdmin }, commandType: CommandType.StoredProcedure).ToList();
            }
            return OrderList;
        }


        [Route("DeleteOrder")]
        [HttpDelete]
        public int DeleteOrder(int orderId)
        {
            int result;
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["DapperConnectionString"].ConnectionString))
            {

                result = db.ExecuteScalar<int>("USP_Delete_Order", new { OrderId = orderId }, commandType: CommandType.StoredProcedure);
            }

            return result;

        }


        [Route("UpdateOrder")]
        [HttpPut]
        public OrderViewModel UpdateOrder(OrderViewModel order)
        {
            OrderViewModel orderViewModel = new OrderViewModel();
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["DapperConnectionString"].ConnectionString))
                {
                    orderViewModel = db.Query<OrderViewModel>("USP_Update_Order", new { Id = order.Id, CustomerName = order.CustomerName, MobileNumber = order.MobileNumber, ShippingAddress = order.ShippingAddress, OrderStatusId = order.OrderStatusId }, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return orderViewModel;
        }


        [Route("CreateOrder")]
        [HttpPost]
        public int CreateOrder(List<OrderViewModel> orderList)
        {
            int result;
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["DapperConnectionString"].ConnectionString))
                {

                    var dt = new DataTable();
                    dt.Columns.Add("CustomerName", typeof(string));
                    dt.Columns.Add("MobileNumber", typeof(long));
                    dt.Columns.Add("ShippingAddress", typeof(string));
                    dt.Columns.Add("Quantity", typeof(long));
                    dt.Columns.Add("ProductId", typeof(long));
                    foreach (var o in orderList)
                    {
                        dt.Rows.Add(o.CustomerName, o.MobileNumber, o.ShippingAddress, o.Quantity, o.ProductId);
                    }

                    result = db.ExecuteScalar<int>("USP_Create_Order", new { TVP = dt.AsTableValuedParameter("dbo.TVP_Order") }, commandType: CommandType.StoredProcedure);

                    if (result > 0) {

                        using (MailMessage mail = new MailMessage())
                        {
                            mail.From = new MailAddress(ConfigurationManager.AppSettings["emailsender"].ToString());
                            mail.To.Add("uday.pintu369@gmail.com");
                            mail.Subject = "Order Confirmation";
                            mail.Body = createEmailBody(orderList[0].CustomerName, "Order Confirmation");
                            mail.IsBodyHtml = true;

                            using (SmtpClient smtp = new SmtpClient(ConfigurationManager.AppSettings["smtpserver"].ToString(), Convert.ToInt32(ConfigurationManager.AppSettings["portnumber"])))
                            {
                                smtp.UseDefaultCredentials = false;
                                smtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["emailsender"].ToString(), ConfigurationManager.AppSettings["password"].ToString());
                                smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["IsSSL"]);
                                smtp.Send(mail);
                            }
                        }

                    }
                    
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                result = -1;
                //we can log Exception here.
            }

            return result;
        }


        private string createEmailBody(string userName, string message)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("/htmlTemplate.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{UserName}", userName);
            body = body.Replace("{message}", message);
            return body;
        }

    }
}