<%@ Page Title="Equipos | Liga Arnaldo Chacon Soto" MetaDescription="Conoce los clubes participantes de la Liga Arnaldo Chacon Soto y consulta sus escudos y categorias." Language="C#" MasterPageFile="~/Vistas/Plantilla.Master" AutoEventWireup="true" CodeBehind="Equipos.aspx.cs" Inherits="SistemaLigaChacon.Vistas.Equipos" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <style>
        .team-crest {
            height: 112px;
            display: flex;
            align-items: center;
            justify-content: center
        }

            .team-crest img {
                max-height: 104px;
                max-width: 100%;
                object-fit: contain
            }

        .team-name {
            min-height: 3rem
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="ContenedorPrincipal" runat="server">
    <div class="app-content-header">
        <div class="container-fluid">
            <div class="row">
                <div class="col-sm-8">
                    <h1 class="mb-1">Equipos</h1>
                    <p class="text-secondary mb-0">Los clubes participantes de la Liga Arnaldo Chacon Soto.</p>
                </div>
                <div class="col-sm-4">
                    <ol class="breadcrumb float-sm-end">
                        <li class="breadcrumb-item"><a href="Home.aspx">Inicio</a></li>
                        <li class="breadcrumb-item active">Equipos</li>
                    </ol>
                </div>
            </div>
        </div>
    </div>
    <section class="content">
        <div class="container-fluid">
            <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false" />
            <div class="card card-primary card-outline">
                <div class="card-header"><h2 class="card-title">Liga Mayor</h2></div>
                <div class="card-body">
                    <div class="row row-cols-2 row-cols-md-3 row-cols-xl-5 g-3">
                        <asp:Repeater ID="rptEquiposMayor" runat="server">
                            <ItemTemplate>
                                <div class="col"><article class="card card-outline card-success h-100"><div class="card-body text-center"><div class="team-crest"><img src='<%# ResolveUrl(Convert.ToString(Eval("LogoUrl"))) %>' alt='<%# System.Web.HttpUtility.HtmlAttributeEncode("Escudo " + Convert.ToString(Eval("Nombre"))) %>' loading="lazy" /></div><h3 class="h6 team-name mt-2"><%#: Eval("Nombre") %></h3></div></article></div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <asp:Panel ID="pnlSinEquiposMayor" runat="server" CssClass="alert alert-light border mb-0" Visible="false">Todavia no hay equipos activos en Liga Mayor.</asp:Panel>
                </div>
            </div>
            <div class="card card-success card-outline">
                <div class="card-header"><h2 class="card-title">Liga Intermedia</h2></div>
                <div class="card-body">
                    <div class="row row-cols-2 row-cols-md-3 row-cols-xl-5 g-3">
                        <asp:Repeater ID="rptEquiposIntermedia" runat="server">
                            <ItemTemplate>
                                <div class="col"><article class="card card-outline card-success h-100"><div class="card-body text-center"><div class="team-crest"><img src='<%# ResolveUrl(Convert.ToString(Eval("LogoUrl"))) %>' alt='<%# System.Web.HttpUtility.HtmlAttributeEncode("Escudo " + Convert.ToString(Eval("Nombre"))) %>' loading="lazy" /></div><h3 class="h6 team-name mt-2"><%#: Eval("Nombre") %></h3></div></article></div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <asp:Panel ID="pnlSinEquiposIntermedia" runat="server" CssClass="alert alert-light border mb-0" Visible="false">Todavia no hay equipos activos en Liga Intermedia.</asp:Panel>
                </div>
            </div>
            <div class="alert alert-light border mt-3 mb-0" role="note">Los clubes y sus categorias se consultan desde la base de datos de la liga.</div>
        </div>
    </section>
</asp:Content>
