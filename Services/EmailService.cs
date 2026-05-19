using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DoctorAppBackend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAppointmentConfirmation(
            string toEmail,
            string patientName,
            string doctorName,
            string slotDate,
            string slotTime,
            decimal fees)
        {
            // ── Format the date nicely ──────────────────
            var dateParts = slotDate.Split('_');
            var months = new[] { "", "Jan", "Feb", "Mar", "Apr", "May",
                                  "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            string formattedDate = $"{dateParts[0]} {months[int.Parse(dateParts[1])]} {dateParts[2]}";

            // ── Build Email ─────────────────────────────
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(
                _config["EmailSettings:FromName"],
                _config["EmailSettings:FromEmail"]
            ));

            email.To.Add(new MailboxAddress(patientName, toEmail));
            email.Subject = "✅ Appointment Confirmed - Prescripto";

            // ── Email Body (HTML) ───────────────────────
            email.Body = new TextPart("html")
            {
                Text = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    
                    <!-- Header -->
                    <div style='background: linear-gradient(135deg, #667eea, #764ba2); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='color: white; margin: 0;'>🏥 Prescripto</h1>
                        <p style='color: white; opacity: 0.9; margin: 5px 0 0 0;'>Your Health, Our Priority</p>
                    </div>

                    <!-- Body -->
                    <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <h2 style='color: #333;'>Appointment Confirmed! ✅</h2>
                        <p style='color: #666;'>Dear <strong>{patientName}</strong>,</p>
                        <p style='color: #666;'>Your appointment has been successfully booked. Here are your details:</p>

                        <!-- Details Card -->
                        <div style='background: white; border-radius: 10px; padding: 20px; margin: 20px 0; border-left: 4px solid #667eea;'>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr style='border-bottom: 1px solid #eee;'>
                                    <td style='padding: 12px 0; color: #888; width: 40%;'>👨‍⚕️ Doctor</td>
                                    <td style='padding: 12px 0; color: #333; font-weight: bold;'>Dr. {doctorName}</td>
                                </tr>
                                <tr style='border-bottom: 1px solid #eee;'>
                                    <td style='padding: 12px 0; color: #888;'>📅 Date</td>
                                    <td style='padding: 12px 0; color: #333; font-weight: bold;'>{formattedDate}</td>
                                </tr>
                                <tr style='border-bottom: 1px solid #eee;'>
                                    <td style='padding: 12px 0; color: #888;'>⏰ Time</td>
                                    <td style='padding: 12px 0; color: #333; font-weight: bold;'>{slotTime}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px 0; color: #888;'>💰 Fees</td>
                                    <td style='padding: 12px 0; color: #667eea; font-weight: bold;'>Rs. {fees}</td>
                                </tr>
                            </table>
                        </div>

                        <p style='color: #666;'>Please arrive 10 minutes before your appointment time.</p>
                        
                        <div style='background: #fff3cd; border-radius: 8px; padding: 15px; margin: 20px 0;'>
                            <p style='color: #856404; margin: 0;'>⚠️ If you need to cancel, please do so at least 2 hours before your appointment.</p>
                        </div>

                        <p style='color: #666;'>Thank you for choosing <strong>Prescripto</strong>!</p>
                    </div>

                    <!-- Footer -->
                    <div style='text-align: center; padding: 20px; color: #aaa; font-size: 12px;'>
                        <p>© 2026 Prescripto. All rights reserved.</p>
                    </div>
                </div>"
            };





            // ── Send Email ──────────────────────────────
            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _config["EmailSettings:FromEmail"],
                _config["EmailSettings:AppPassword"]
            );

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);






        }


        public async Task SendDoctorNotification(
    string doctorEmail,
    string doctorName,
    string patientName,
    string slotDate,
    string slotTime,
    decimal fees)
        {
            // ── Format date ─────────────────────────────────
            var dateParts = slotDate.Split('_');
            var months = new[] { "", "Jan", "Feb", "Mar", "Apr", "May",
                          "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            string formattedDate = $"{dateParts[0]} {months[int.Parse(dateParts[1])]} {dateParts[2]}";

            // ── Build Email ──────────────────────────────────
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(
                _config["EmailSettings:FromName"],
                _config["EmailSettings:FromEmail"]
            ));

            email.To.Add(new MailboxAddress(doctorName, doctorEmail));
            email.Subject = "🔔 New Appointment Booked - Prescripto";

            email.Body = new TextPart("html")
            {
                Text = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>

            <!-- Header -->
            <div style='background: linear-gradient(135deg, #667eea, #764ba2); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                <h1 style='color: white; margin: 0;'>🏥 Prescripto</h1>
                <p style='color: white; opacity: 0.9; margin: 5px 0 0 0;'>Doctor Notification</p>
            </div>

            <!-- Body -->
            <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                <h2 style='color: #333;'>New Appointment! 🔔</h2>
                <p style='color: #666;'>Dear <strong>Dr. {doctorName}</strong>,</p>
                <p style='color: #666;'>A patient has booked an appointment with you. Here are the details:</p>

                <!-- Details Card -->
                <div style='background: white; border-radius: 10px; padding: 20px; margin: 20px 0; border-left: 4px solid #667eea;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr style='border-bottom: 1px solid #eee;'>
                            <td style='padding: 12px 0; color: #888; width: 40%;'>🤒 Patient</td>
                            <td style='padding: 12px 0; color: #333; font-weight: bold;'>{patientName}</td>
                        </tr>
                        <tr style='border-bottom: 1px solid #eee;'>
                            <td style='padding: 12px 0; color: #888;'>📅 Date</td>
                            <td style='padding: 12px 0; color: #333; font-weight: bold;'>{formattedDate}</td>
                        </tr>
                        <tr style='border-bottom: 1px solid #eee;'>
                            <td style='padding: 12px 0; color: #888;'>⏰ Time</td>
                            <td style='padding: 12px 0; color: #333; font-weight: bold;'>{slotTime}</td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; color: #888;'>💰 Fees</td>
                            <td style='padding: 12px 0; color: #667eea; font-weight: bold;'>Rs. {fees}</td>
                        </tr>
                    </table>
                </div>

                <p style='color: #666;'>Please be available at the scheduled time.</p>

                <div style='background: #d4edda; border-radius: 8px; padding: 15px; margin: 20px 0;'>
                    <p style='color: #155724; margin: 0;'>✅ Login to your dashboard to view and manage all appointments.</p>
                </div>

                <p style='color: #666;'>Thank you for being part of <strong>Prescripto</strong>!</p>
            </div>

            <!-- Footer -->
            <div style='text-align: center; padding: 20px; color: #aaa; font-size: 12px;'>
                <p>© 2026 Prescripto. All rights reserved.</p>
            </div>
        </div>"
            };

            // ── Send Email ───────────────────────────────────
            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _config["EmailSettings:FromEmail"],
                _config["EmailSettings:AppPassword"]
            );

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    



   

    public async Task SendDoctorApprovalEmail(string doctorEmail, string doctorName)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _config["EmailSettings:FromName"],
                _config["EmailSettings:FromEmail"]
            ));
            email.To.Add(new MailboxAddress(doctorName, doctorEmail));
            email.Subject = "✅ Your Account Has Been Approved - Prescripto";

            email.Body = new TextPart("html")
            {
                Text = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background: linear-gradient(135deg, #667eea, #764ba2); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                <h1 style='color: white; margin: 0;'>🏥 Prescripto</h1>
                <p style='color: white; opacity: 0.9; margin: 5px 0 0 0;'>Doctor Portal</p>
            </div>
            <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                <h2 style='color: #333;'>Congratulations! Your Account is Approved ✅</h2>
                <p style='color: #666;'>Dear <strong>Dr. {doctorName}</strong>,</p>
                <p style='color: #666;'>We are pleased to inform you that your account on <strong>Prescripto</strong> has been reviewed and approved by our admin team.</p>
                <div style='background: #d4edda; border-radius: 8px; padding: 15px; margin: 20px 0;'>
                    <p style='color: #155724; margin: 0;'>✅ You can now login to your dashboard and start receiving patient appointments.</p>
                </div>
                <p style='color: #666;'>Welcome to the Prescripto family!</p>
                <p style='color: #666;'>Thank you for choosing <strong>Prescripto</strong>!</p>
            </div>
            <div style='text-align: center; padding: 20px; color: #aaa; font-size: 12px;'>
                <p>© 2026 Prescripto. All rights reserved.</p>
            </div>
        </div>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(
                _config["EmailSettings:FromEmail"],
                _config["EmailSettings:AppPassword"]
            );
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendDoctorRejectionEmail(string doctorEmail, string doctorName)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _config["EmailSettings:FromName"],
                _config["EmailSettings:FromEmail"]
            ));
            email.To.Add(new MailboxAddress(doctorName, doctorEmail));
            email.Subject = "❌ Your Account Application - Prescripto";

            email.Body = new TextPart("html")
            {
                Text = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background: linear-gradient(135deg, #667eea, #764ba2); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                <h1 style='color: white; margin: 0;'>🏥 Prescripto</h1>
                <p style='color: white; opacity: 0.9; margin: 5px 0 0 0;'>Doctor Portal</p>
            </div>
            <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                <h2 style='color: #333;'>Account Application Update</h2>
                <p style='color: #666;'>Dear <strong>Dr. {doctorName}</strong>,</p>
                <p style='color: #666;'>Thank you for applying to join <strong>Prescripto</strong>. After reviewing your profile, we regret to inform you that your application has not been approved at this time.</p>
                <div style='background: #f8d7da; border-radius: 8px; padding: 15px; margin: 20px 0;'>
                    <p style='color: #721c24; margin: 0;'>❌ Your account has been rejected. If you believe this is a mistake, please contact our support team.</p>
                </div>
                <p style='color: #666;'>You may reapply after updating your profile with complete and accurate information.</p>
                <p style='color: #666;'>Thank you for your interest in <strong>Prescripto</strong>.</p>
            </div>
            <div style='text-align: center; padding: 20px; color: #aaa; font-size: 12px;'>
                <p>© 2026 Prescripto. All rights reserved.</p>
            </div>
        </div>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(
                _config["EmailSettings:FromEmail"],
                _config["EmailSettings:AppPassword"]
            );
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendNewDoctorProfileNotification(
    string adminEmail,
    string doctorName,
    string doctorEmail,
    string speciality,
    string degree,
    string experience)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _config["EmailSettings:FromName"],
                _config["EmailSettings:FromEmail"]
            ));
            email.To.Add(new MailboxAddress("Admin", adminEmail));
            email.Subject = "🔔 New Doctor Profile Submitted - Action Required";

            email.Body = new TextPart("html")
            {
                Text = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background: linear-gradient(135deg, #667eea, #764ba2); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                <h1 style='color: white; margin: 0;'>🏥 Prescripto</h1>
                <p style='color: white; opacity: 0.9; margin: 5px 0 0 0;'>Admin Notification</p>
            </div>
            <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                <h2 style='color: #333;'>New Doctor Profile Submitted 🔔</h2>
                <p style='color: #666;'>A new doctor has submitted their profile and is waiting for your approval.</p>

                <div style='background: white; border-radius: 10px; padding: 20px; margin: 20px 0; border-left: 4px solid #667eea;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr style='border-bottom: 1px solid #eee;'>
                            <td style='padding: 12px 0; color: #888; width: 40%;'>👨‍⚕️ Doctor Name</td>
                            <td style='padding: 12px 0; color: #333; font-weight: bold;'>Dr. {doctorName}</td>
                        </tr>
                        <tr style='border-bottom: 1px solid #eee;'>
                            <td style='padding: 12px 0; color: #888;'>📧 Email</td>
                            <td style='padding: 12px 0; color: #333; font-weight: bold;'>{doctorEmail}</td>
                        </tr>
                        <tr style='border-bottom: 1px solid #eee;'>
                            <td style='padding: 12px 0; color: #888;'>🏥 Speciality</td>
                            <td style='padding: 12px 0; color: #333; font-weight: bold;'>{speciality}</td>
                        </tr>
                        <tr style='border-bottom: 1px solid #eee;'>
                            <td style='padding: 12px 0; color: #888;'>🎓 Degree</td>
                            <td style='padding: 12px 0; color: #333; font-weight: bold;'>{degree}</td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; color: #888;'>⏳ Experience</td>
                            <td style='padding: 12px 0; color: #333; font-weight: bold;'>{experience}</td>
                        </tr>
                    </table>
                </div>

                <div style='background: #fff3cd; border-radius: 8px; padding: 15px; margin: 20px 0;'>
                    <p style='color: #856404; margin: 0;'>⚠️ Please login to your admin dashboard to review and approve or reject this doctor.</p>
                </div>

                <p style='color: #666;'>Thank you for managing <strong>Prescripto</strong>!</p>
            </div>
            <div style='text-align: center; padding: 20px; color: #aaa; font-size: 12px;'>
                <p>© 2026 Prescripto. All rights reserved.</p>
            </div>
        </div>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(
                _config["EmailSettings:FromEmail"],
                _config["EmailSettings:AppPassword"]
            );
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
