<%@ Page Title="Usuarios | Liga Arnaldo Chacon Soto" Language="C#" MasterPageFile="~/Vistas/Plantilla.Master" AutoEventWireup="true" CodeBehind="AdministrarUsuarios.aspx.cs" Inherits="SistemaLigaChacon.Vistas.AdministrarUsuarios" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <meta name="robots" content="noindex, nofollow" />
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="ContenedorPrincipal" runat="server">
    <div class="app-content-header">
        <div class="container-fluid">
            <h1 class="mb-1">Usuarios</h1>
            <p class="text-secondary mb-0">Solo el superadministrador crea cuentas de administrador y consulta.</p>
        </div>
    </div>
    <section class="content">
        <div class="container-fluid">
            <asp:Label ID="lblMensaje" runat="server" CssClass="alert alert-info d-block" Visible="false" />
            <div class="card card-primary card-outline">
                <div class="card-header">
                    <h2 class="card-title">Crear cuenta</h2>
                </div>
                <div class="card-body">
                    <div class="row g-3 align-items-end">
                        <div class="col-12 col-md-3">
                            <label class="form-label" for="txtNombre">Nombre</label>
                            <asp:TextBox ID="txtNombre" runat="server" MaxLength="120" CssClass="form-control" />
                        </div>
                        <div class="col-12 col-md-3">
                            <label class="form-label" for="txtUsuario">Usuario</label>
                            <asp:TextBox ID="txtUsuario" runat="server" MaxLength="60" CssClass="form-control" />
                        </div>
                        <div class="col-12 col-md-3">
                            <label class="form-label" for="txtContrasena">Contrasena (minimo 10 caracteres)</label>
                            <asp:TextBox ID="txtContrasena" runat="server" TextMode="Password" MaxLength="200" CssClass="form-control" />
                        </div>
                        <div class="col-12 col-md-2">
                            <label class="form-label" for="ddlRol">Rol</label>
                            <asp:DropDownList ID="ddlRol" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Administrador" Value="ADMINISTRADOR" />
                                <asp:ListItem Text="Usuario" Value="USUARIO" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-12 col-md-1">
                            <asp:Button ID="btnCrear" runat="server" Text="Crear" CssClass="btn btn-primary" OnClick="btnCrear_Click" />
                        </div>
                    </div>
                </div>
            </div>
            <div class="card">
                <div class="card-header">
                    <h2 class="card-title">Cuentas registradas</h2>
                </div>
                <div class="card-body table-responsive">
                    <asp:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="false" CssClass="table table-striped align-middle" GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="NombreMostrar" HeaderText="Nombre" />
                            <asp:BoundField DataField="NombreUsuario" HeaderText="Usuario" />
                            <asp:BoundField DataField="Rol" HeaderText="Rol" />
                            <asp:BoundField DataField="Activo" HeaderText="Activo" />
                            <asp:BoundField DataField="CreadoEn" HeaderText="Creado" DataFormatString="{0:dd/MM/yyyy}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
