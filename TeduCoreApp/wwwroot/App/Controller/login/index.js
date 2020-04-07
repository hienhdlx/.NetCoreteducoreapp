var loginController = function() {
    this.intialize = function() {
        registerEvents();
    };

    var registerEvents = function () {

        $('#frmLogin').validate({
            errorClass: 'red',
            ignore: [],
            lang: 'en',
            rules: {
                userName: {
                    required: true
                },
                password: {
                    required: true
                }
            }
        });

        $('#txtPass').on('keypress',
            function(e) {
                if (e.which === 13) {
                    if ($('#frmLogin').valid()) {
                        e.preventDefault();
                        var user = $("#txtUser").val();
                        var pass = $("#txtPass").val();
                        login(user, pass);
                    }
                }
            });
       
        $("#btnLogin").on('click',
            function (e) {
                if ($('#frmLogin').valid()) {
                    e.preventDefault();
                    var user = $("#txtUser").val();
                    var pass = $("#txtPass").val();
                    login(user, pass);
                }
            });
    }; 

    var login = function(user, pass) {
        $.ajax({
            type: 'POST',
            data: {
                UserName: user,
                Password: pass
            },
            dataType: 'json', 
            url: '/admin/login/authen',
            success: function (res) {
                if (res.Success) {
                    window.location.href = "/Admin/Home/Index";
                } else {
                    tedu.notify("Login failed", "error");
                }
            }
        });
    }
}