<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Student Portal | AIT Aurangabad</title>

    <!-- Performance: Preconnect to external font domains -->
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;700&family=Inter:wght@300;400;500;600&display=swap" rel="stylesheet" />
    
    <!-- Fix: Favicon 404 Error -->
    <link rel="icon" href="data:;base64,iVBORw0KGgo=" />

    <!-- CSS Dependencies -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>


    <style>
        :root {
            --ait-sky-blue: #87CEEB;
            --ait-royal-blue: #2563eb;
            --ait-dark-blue: #1d2d3b;
            --ait-accent-blue: #f97316;
            --ait-text: #2e3a45;
            --white: #ffffff;
        }

        html, body {
            width: 100%;
            overflow-x: hidden;
            margin: 0;
            padding: 0;
        }

        body {
            font-family: 'Inter', sans-serif;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: linear-gradient(-45deg, var(--ait-sky-blue), var(--white), var(--ait-sky-blue), var(--white));
            background-size: 400% 400%;
            animation: gradient-animation 15s ease infinite;
        }

        @keyframes gradient-animation {
            0% { background-position: 0% 50%; }
            50% { background-position: 100% 50%; }
            100% { background-position: 0% 50%; }
        }

        .login-wrapper {
            width: 100%;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
            min-height: 100vh;
            box-sizing: border-box;
        }

        .login-card {
            background: rgba(255, 255, 255, 0.85);
            backdrop-filter: blur(20px);
            -webkit-backdrop-filter: blur(20px);
            border: 1px solid rgba(255, 255, 255, 0.5);
            border-radius: 28px;
            padding: 40px;
            width: 100%;
            max-width: 450px;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.15);
            position: relative;
            transition: all 0.3s ease;
            box-sizing: border-box;
        }

        .login-card::before {
            content: "";
            position: absolute;
            top: 0;
            left: 50%;
            transform: translateX(-50%);
            width: 120px;
            height: 5px;
            background: var(--ait-accent-orange);
            border-radius: 0 0 10px 10px;
        }

        .institute-brand {
            text-align: center;
            margin-bottom: 30px;
        }

        /* Brand Logo Container & Animation */
        .brand-logo-container {
            display: flex;
            justify-content: center;
            align-items: center;
            margin-bottom: 15px;
        }

        .brand-logo-img {
            height: clamp(70px, 15vw, 90px); /* Slightly larger as the main brand element */
            width: auto;
            filter: drop-shadow(0 8px 15px rgba(0,0,0,0.1));
            max-width: 100%;
            object-fit: contain;
            /* Animation: Subtle Professional Pulse */
            animation: brand-pulse 4s infinite ease-in-out;
            will-change: transform, opacity;
        }

        @keyframes brand-pulse {
            0%, 100% { 
                transform: scale(1);
                opacity: 1;
            }
            50% { 
                transform: scale(1.03);
                opacity: 0.95;
            }
        }

        .login-title {
            font-family: 'Poppins', sans-serif;
            font-weight: 700;
            color: var(--ait-dark-blue);
            font-size: clamp(1.4rem, 5.5vw, 1.7rem);
            margin-top: 5px;
            margin-bottom: 4px;
        }

        .login-subtitle {
            color: var(--ait-text);
            font-size: clamp(0.75rem, 3vw, 0.85rem);
            opacity: 0.8;
            font-weight: 500;
        }

        .form-group-official {
            margin-bottom: 24px;
        }

        .input-label {
            display: block;
            font-size: 0.75rem;
            font-weight: 600;
            color: var(--ait-dark-blue);
            margin-bottom: 8px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .input-box {
            position: relative;
            display: flex;
            align-items: center;
        }

        .input-box i.input-icon {
            position: absolute;
            left: 16px;
            color: #94a3b8;
            font-size: 1.1rem;
            z-index: 5;
        }

        .lock-icon-container {
            position: absolute;
            left: 16px;
            display: flex;
            align-items: center;
            z-index: 5;
        }

        .fa-lock.animated-lock {
            color: #94a3b8;
            font-size: 1.1rem;
            animation: lock-wiggle 5s infinite;
        }

        @keyframes lock-wiggle {
            0%, 80%, 100% { transform: rotate(0deg); }
            85% { transform: rotate(-10deg); }
            90% { transform: rotate(10deg); }
            95% { transform: rotate(0deg); }
        }

        .form-control-ait {
            width: 100%;
            padding: 14px 16px 14px 48px;
            background: rgba(255, 255, 255, 0.9);
            border: 1.5px solid #e2e8f0;
            border-radius: 14px;
            font-size: 16px; 
            color: var(--ait-dark-blue);
            transition: all 0.3s ease;
        }

        .form-control-ait:focus {
            border-color: var(--ait-royal-blue);
            background: white;
            box-shadow: 0 0 0 4px rgba(37, 99, 235, 0.1);
            outline: none;
        }

        .toggle-password {
            position: absolute;
            right: 16px;
            cursor: pointer;
            color: #94a3b8;
            font-size: 1rem;
            z-index: 10;
            padding: 5px; 
            border: none;
            background: transparent;
        }

        .btn-login-ait {
            background: var(--ait-royal-blue);
            color: white;
            border: none;
            width: 100%;
            padding: 15px;
            border-radius: 14px;
            font-weight: 600;
            font-size: 1rem;
            text-transform: uppercase;
            letter-spacing: 1.5px;
            transition: all 0.3s;
            margin-top: 10px;
            box-shadow: 0 10px 20px -5px rgba(37, 99, 235, 0.4);
            cursor: pointer;
        }

        .btn-login-ait:hover {
            background: var(--ait-dark-blue);
            transform: translateY(-2px);
            box-shadow: 0 15px 25px -5px rgba(29, 45, 59, 0.3);
        }

        .error-tag {
            display: inline-block;
            background: #fff1f2;
            color: #e11d48;
            padding: 8px 16px;
            border-radius: 10px;
            font-size: 0.8rem;
            border: 1px solid #fecdd3;
            margin-top: 20px;
            width: 100%;
            box-sizing: border-box;
        }

        .login-footer {
            text-align: center;
            margin-top: 35px;
            font-size: 0.85rem;
            color: var(--ait-text);
        }

        .login-footer p {
            margin-bottom: 8px;
        }

        .login-footer a {
            color: var(--ait-royal-blue);
            text-decoration: none;
            font-weight: 600;
        }

        .animate-up {
            animation: slideUp 0.8s cubic-bezier(0.2, 0.8, 0.2, 1) both;
        }

        @keyframes slideUp {
            from { opacity: 0; transform: translateY(30px); }
            to { opacity: 1; transform: translateY(0); }
        }

        @media (max-width: 480px) {
            .login-card {
                padding: 30px 20px;
                border-radius: 20px;
                margin: 10px;
            }
            .login-footer {
                margin-top: 25px;
            }
            .form-group-official {
                margin-bottom: 18px;
            }
            .btn-login-ait {
                padding: 12px;
                font-size: 0.9rem;
            }
        }

        @media (max-height: 700px) and (orientation: landscape) {
            .login-wrapper {
                padding: 40px 20px;
            }
            body {
                overflow-y: auto;
            }
        }

        .login-message {
    font-weight: bold;
    padding: 10px;
    border-radius: 5px;
    display: inline-block;
    margin-top: 10px;
}

.error {
    background-color: #f8d7da;
    color: #721c24;
    border: 1px solid #f5c6cb;
}

.warning {
    background-color: #fff3cd;
    color: #856404;
    border: 1px solid #ffeeba;
}

.success {
    background-color: #d4edda;
    color: #155724;
    border: 1px solid #c3e6cb;
}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-wrapper">
            <div class="login-card animate-up">
                <div class="institute-brand">
                    <div class="brand-logo-container">
                        <img src="New_Ait_Logo.png" alt="AIT Logo" class="brand-logo-img" 
                             loading="eager" 
                             onerror="this.onerror=null; this.src='New_Ait_Logo.png'" />
                    </div>
                         
                    <%--<h2 class="login-title">Student Portal</h2>--%>
                    <%--<p class="login-subtitle">Academy Of Information Technology</p>--%>
                </div>

                <div class="form-group-official">
                    <label for="txtEmailID" class="input-label">Student ID / Email</label>
                    <div class="input-box">
                        <i class="fas fa-user-circle input-icon" aria-hidden="true"></i>
                        <asp:TextBox ID="txtEmailID" runat="server" CssClass="form-control-ait" placeholder="Enter your ID" autocomplete="off"></asp:TextBox>
                    </div>
                </div>

                <div class="form-group-official">
                    <label for="txtPassword" class="input-label">Password</label>
                    <div class="input-box">
                        <div class="lock-icon-container">
                            <i class="fas fa-lock animated-lock" aria-hidden="true"></i>
                        </div>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control-ait" TextMode="Password" placeholder="••••••••"></asp:TextBox>
                        <button type="button" id="toggleIcon" class="fas fa-eye-slash toggle-password" onclick="togglePassword()" aria-label="Toggle Password Visibility"></button>
                    </div>
                </div>

                <asp:Button ID="Button1" runat="server" Text="Login In Securely" CssClass="btn-login-ait" OnClick="Button1_Click" />

                <asp:Panel ID="pnlError" runat="server" Visible="false" style="text-align:center;">
                    <div class="error-tag">
                        <i class="fas fa-exclamation-triangle me-1"></i>
                        <asp:Label ID="lblError" runat="server" CssClass="login-message"></asp:Label>

                    </div>
                </asp:Panel>

                <div class="login-footer">
                    <%--<p>Problems signing in? <a href="#">Help Center</a></p>--%>
                    <p>&copy; 2026 Academy of Information Technology (AIT)<br /> All Rights Reserved</p>
                </div>
            </div>
        </div>
    </form>

    <script defer="defer">
        (function () {
            window.togglePassword = function () {
                const passwordField = document.getElementById('<%= txtPassword.ClientID %>');
                const btn = document.getElementById('toggleIcon');

                if (passwordField && btn) {
                    if (passwordField.type === "password") {
                        passwordField.type = "text";
                        btn.classList.replace('fa-eye-slash', 'fa-eye');
                    } else {
                        passwordField.type = "password";
                        btn.classList.replace('fa-eye', 'fa-eye-slash');
                    }
                }
            };

            document.addEventListener('DOMContentLoaded', function () {
                const loginBtn = document.getElementById('<%= Button1.ClientID %>');

                if (loginBtn) {
                    loginBtn.addEventListener('click', function () {
                        if (typeof Page_ClientValidate === 'undefined' || Page_ClientValidate()) {
                            this.value = 'VERIFYING...';
                            this.style.opacity = '0.7';
                            this.style.pointerEvents = 'none';
                        }
                    });
                }
            });
        })();
    </script>
</body>
</html>