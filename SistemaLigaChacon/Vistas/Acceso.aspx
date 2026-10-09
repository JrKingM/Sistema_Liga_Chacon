<%@ Page Title="Iniciar sesion | Liga Arnaldo Chacon Soto" MetaDescription="Inicia sesion en el portal de la Liga Arnaldo Chacon Soto." Language="C#" MasterPageFile="~/Vistas/Plantilla.Master" AutoEventWireup="true" CodeBehind="Acceso.aspx.cs" Inherits="SistemaLigaChacon.Vistas.Acceso" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <meta name="robots" content="noindex, nofollow" />
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="ContenedorPrincipal" runat="server">
    <div class="row justify-content-center">
        <div class="col-12 col-md-8 col-lg-6">
            <div class="card card-success card-outline">
                <div class="card-header">
                    <h1 class="card-title h4">Acceso al portal</h1>
                </div>
                <div class="card-body">
                    <asp:Label ID="lblMensaje" runat="server" CssClass="alert alert-danger d-block" Visible="false" />
                    <asp:Panel ID="pnlConfiguracionInicial" runat="server" Visible="false">
                        <h2 class="h5">Crear la primera cuenta superadministradora</h2>
                        <p class="text-secondary">Esta opcion solo aparece mientras no exista ninguna cuenta. Guarda bien estos datos.</p>
                        <div class="mb-3">
                            <label class="form-label" for="txtNombreMostrar">Nombre</label>
                            <asp:TextBox ID="txtNombreMostrar" runat="server" MaxLength="120" CssClass="form-control" autocomplete="name" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label" for="txtNuevoUsuario">Usuario</label>
                            <asp:TextBox ID="txtNuevoUsuario" runat="server" MaxLength="60" CssClass="form-control" autocomplete="username" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label" for="txtNuevaContrasena">Contrasena (minimo 10 caracteres)</label>
                            <asp:TextBox ID="txtNuevaContrasena" runat="server" TextMode="Password" MaxLength="200" CssClass="form-control" autocomplete="new-password" />
                        </div>
                        <asp:Button ID="btnCrearAdministrador" runat="server" Text="Crear superadministrador" CssClass="btn btn-success" OnClick="btnCrearAdministrador_Click" />
                    </asp:Panel>
                    <asp:Panel ID="pnlLogin" runat="server" Visible="false">
                        <div class="mb-3">
                            <label class="form-label" for="txtUsuario">Usuario</label>
                            <asp:TextBox ID="txtUsuario" runat="server" MaxLength="60" CssClass="form-control" autocomplete="username" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label" for="txtContrasena">Contrasena</label>
                            <asp:TextBox ID="txtContrasena" runat="server" TextMode="Password" MaxLength="200" CssClass="form-control" autocomplete="current-password" />
                        </div>
                        <asp:Button ID="btnIngresar" runat="server" Text="Iniciar sesion" CssClass="btn btn-success" OnClick="btnIngresar_Click" />
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
