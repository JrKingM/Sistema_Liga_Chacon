<%@ Page Title="Campeones | Liga Arnaldo Chacon Soto" MetaDescription="Consulta el archivo historico de equipos campeones, goleadores y porteros destacados de la Liga Arnaldo Chacon Soto." Language="C#" MasterPageFile="~/Vistas/Plantilla.Master" AutoEventWireup="true" CodeBehind="Campeones.aspx.cs" Inherits="SistemaLigaChacon.Vistas.Campeones" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <style>
        .champion-crest {
            height: 120px;
            display: flex;
            align-items: center;
            justify-content: center
        }

            .champion-crest img {
                max-height: 112px;
                max-width: 100%;
                object-fit: contain
            }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="ContenedorPrincipal" runat="server">
    <div class="app-content-header">
        <div class="container-fluid">
            <div class="row">
                <div class="col-sm-8">
                    <h1 class="mb-1">Equipos campeones</h1>
                    <p class="text-secondary mb-0">Archivo historico de campeones, goleadores y porteros por temporada y categoria.</p>
                </div>
                <div class="col-sm-4">
                    <ol class="breadcrumb float-sm-end">
                        <li class="breadcrumb-item">
                            <a href="Home.aspx">Inicio</a>
                        </li>
                        <li class="breadcrumb-item active">Campeones</li>
                    </ol>
                </div>
            </div>
        </div>
    </div>
    <section class="content">
        <div class="container-fluid">
            <asp:Label ID="lblAviso" runat="server" CssClass="alert alert-info d-block" Visible="false" />
            <asp:Panel ID="pnlVacio" runat="server" CssClass="card card-outline card-warning">
                <div class="card-body text-center py-5">
                    <div class="display-6 text-warning">★</div>
                    <h2 class="h5 mt-2">Pronto tendremos un campeon</h2>
                    <p class="text-secondary mb-0">Al finalizar el torneo, la administracion registrara al campeon, goleador y portero destacado. Los registros se conservaran aqui.</p>
                </div>
            </asp:Panel>
            <asp:Repeater ID="rptCampeones" runat="server">
                <HeaderTemplate>
                    <div class="row row-cols-1 row-cols-md-2 row-cols-xl-3 g-3">
                </HeaderTemplate>
                <ItemTemplate>
                    <div class="col">
                        <article class="card card-outline card-warning h-100">
                            <div class="card-body text-center">
                                <div class="text-uppercase text-secondary small fw-semibold"><%#: Eval("Temporada") %></div>
                                <div class="display-6 text-warning">★</div>
                                <h2 class="h5"><%#: Eval("Equipo") %></h2>
                                <span class="badge text-bg-success"><%#: Eval("Categoria") %></span>
                                <div class="mt-3 small text-secondary">Portero destacado</div>
                                <div class="fw-semibold"><%#: String.IsNullOrWhiteSpace(Convert.ToString(Eval("Portero"))) ? "No registrado" : Eval("Portero") %></div>
                                <div class="mt-3 small text-secondary">Goleador de la temporada</div>
                                <div class="fw-semibold"><%#: String.IsNullOrWhiteSpace(Convert.ToString(Eval("Goleador"))) ? "No registrado" : Eval("Goleador") %></div>
                            </div>
                        </article>
                    </div>
                </ItemTemplate>
                <FooterTemplate>
                    </div>
                </FooterTemplate>
            </asp:Repeater>
            <asp:Panel ID="pnlAdmin" runat="server" CssClass="card card-primary card-outline mt-4" Visible="false">
                <div class="card-header">
                    <h2 class="card-title">Archivar campeones y premios</h2>
                </div>
                <div class="card-body">
                    <p class="text-secondary small">Registra el campeon, el goleador y el portero destacado al finalizar la temporada.</p>
                    <div class="row g-3 align-items-end">
                        <div class="col-12 col-md-4">
                            <label class="form-label" for="txtTemporada">Temporada</label>
                            <asp:TextBox ID="txtTemporada" runat="server" MaxLength="30" placeholder="Ej. 2026" CssClass="form-control" />
                        </div>
                        <div class="col-12 col-md-4">
                            <label class="form-label" for="ddlCategoria">Categoria</label>
                            <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Mayor" Value="Mayor" />
                                <asp:ListItem Text="Intermedia" Value="Intermedia" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-12 col-md-4">
                            <label class="form-label" for="txtEquipo">Equipo campeon</label>
                            <asp:TextBox ID="txtEquipo" runat="server" MaxLength="100" CssClass="form-control" />
                        </div>
                        <div class="col-12 col-md-6">
                            <label class="form-label" for="txtGoleador">Goleador de la temporada</label>
                            <asp:TextBox ID="txtGoleador" runat="server" MaxLength="100" CssClass="form-control" />
                        </div>
                        <div class="col-12 col-md-6">
                            <label class="form-label" for="txtPortero">Portero destacado</label>
                            <asp:TextBox ID="txtPortero" runat="server" MaxLength="100" CssClass="form-control" />
                        </div>
                        <div class="col-12">
                            <asp:Button ID="btnGuardar" runat="server" Text="Guardar en el archivo historico" CssClass="btn btn-primary" OnClick="btnGuardar_Click" />
                            <asp:Label ID="lblResultado" runat="server" CssClass="small ms-3" />
                        </div>
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlNoAdmin" runat="server" CssClass="alert alert-secondary mt-4" Visible="false">El registro de campeones esta disponible para administracion; el archivo historico es publico.</asp:Panel>
        </div>
    </section>
</asp:Content>
