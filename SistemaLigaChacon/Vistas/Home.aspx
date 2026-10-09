<%@ Page Title="Liga Arnaldo Chacon Soto | Calendario, resultados y posiciones" MetaDescription="Sigue la Liga Arnaldo Chacon Soto: posiciones, calendario, resultados y los clubes participantes en las categorias Mayor e Intermedia." Language="C#" MasterPageFile="~/Vistas/Plantilla.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="SistemaLigaChacon.Vistas.Home" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="application/ld+json">
    {"@context":"https://schema.org","@type":"SportsOrganization","name":"Liga Arnaldo Chacon Soto","url":"<%= Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/Vistas/Home.aspx") %>","logo":"<%= Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/Images/Logo Liga.jpeg") %>","sport":"Soccer","description":"Liga de futbol con categorias Mayor e Intermedia."}
    </script>
    <style>
        .club-crest {
            height: 90px;
            display: flex;
            align-items: center;
            justify-content: center
        }

            .club-crest img {
                max-width: 100%;
                max-height: 82px;
                object-fit: contain
            }

        .empty-state {
            min-height: 130px;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            text-align: center
        }

            .empty-state i {
                font-size: 1.8rem;
                color: #83928a
            }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContenedorPrincipal" runat="server">
    <div class="app-content-header">
        <div class="container-fluid">
            <div class="row">
                <div class="col-sm-8">
                    <h1 class="mb-1">Liga Arnaldo Chacon Soto</h1>
                    <p class="text-secondary mb-0">Portal oficial - Una temporada - Categorias Intermedia y Mayor</p>
                </div>
                <div class="col-sm-4">
                    <ol class="breadcrumb float-sm-end">
                        <li class="breadcrumb-item active">Inicio</li>
                    </ol>
                </div>
            </div>
        </div>
    </div>
    <section class="content">
        <div class="container-fluid">
            <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false" />
            <div class="card bg-success text-white">
                <div class="card-body py-4">
                    <div class="row align-items-center">
                        <div class="col-lg-8">
                            <div class="text-uppercase small fw-semibold opacity-75">Futbol local</div>
                            <h2 class="display-6 fw-bold mb-2">Una liga, dos categorias</h2>
                            <p class="mb-0">Sigue el calendario, consulta las posiciones y conoce a los clubes participantes.</p>
                        </div>
                        <div class="col-lg-4 text-lg-end mt-3 mt-lg-0">
                            <a class="btn btn-light text-success fw-semibold" href="Posiciones.aspx">Ver posiciones</a>
                            <a class="btn btn-outline-light ms-2" href="Equipos.aspx">Ver equipos</a>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-12 col-sm-6 col-xl-3">
                    <div class="info-box">
                        <span class="info-box-icon text-bg-primary">
                            <i class="bi bi-shield"></i>
                        </span>
                        <div class="info-box-content">
                            <span class="info-box-text">Clubes registrados</span>
                            <span class="info-box-number">
                                <asp:Literal ID="litClubCount" runat="server" Text="0" />
                            </span>
                        </div>
                    </div>
                </div>
                <div class="col-12 col-sm-6 col-xl-3">
                    <div class="info-box">
                        <span class="info-box-icon text-bg-success">
                            <i class="bi bi-diagram-3"></i>
                        </span>
                        <div class="info-box-content">
                            <span class="info-box-text">Categorias</span>
                            <span class="info-box-number">2</span>
                        </div>
                    </div>
                </div>
                <div class="col-12 col-sm-6 col-xl-3">
                    <div class="info-box">
                        <span class="info-box-icon text-bg-warning">
                            <i class="bi bi-calendar3"></i>
                        </span>
                        <div class="info-box-content">
                            <span class="info-box-text">Jornadas</span>
                            <span class="info-box-number">9</span>
                        </div>
                    </div>
                </div>
                <div class="col-12 col-sm-6 col-xl-3">
                    <div class="info-box">
                        <span class="info-box-icon text-bg-danger">
                            <i class="bi bi-dribbble"></i>
                        </span>
                        <div class="info-box-content">
                            <span class="info-box-text">Partidos por club</span>
                            <span class="info-box-number">18</span>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-8">
                    <div class="card card-success card-outline">
                        <div class="card-header d-flex flex-wrap align-items-center justify-content-between gap-2">
                            <h2 class="card-title">Tabla de posiciones -
                                <asp:Literal ID="litCategoria" runat="server" /></h2>
                            <div class="btn-group" role="group" aria-label="Seleccionar categoria">
                                <asp:HyperLink ID="lnkMayor" runat="server" CssClass="btn btn-outline-success" Text="Mayor" />
                                <asp:HyperLink ID="lnkIntermedia" runat="server" CssClass="btn btn-outline-success" Text="Intermedia" />
                            </div>
                        </div>
                        <div class="card-body p-0">
                            <div class="table-responsive">
                                <table class="table table-striped table-hover align-middle mb-0">
                                    <thead class="table-light">
                                        <tr>
                                            <th>#</th>
                                            <th>Equipo</th>
                                            <th class="text-center">PJ</th>
                                            <th class="text-center">PG</th>
                                            <th class="text-center">PE</th>
                                            <th class="text-center">PP</th>
                                            <th class="text-center">DG</th>
                                            <th class="text-center">PTS</th>
                                        </tr>
                                    </thead>
                                    <asp:Repeater ID="rptPosiciones" runat="server">
                                        <HeaderTemplate>
                                            <tbody>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <tr>
                                                <td><%# Container.ItemIndex + 1 %></td>
                                                <th scope="row"><%#: Eval("Equipo") %></th>
                                                <td class="text-center"><%#: Eval("PJ") %></td>
                                                <td class="text-center"><%#: Eval("PG") %></td>
                                                <td class="text-center"><%#: Eval("PE") %></td>
                                                <td class="text-center"><%#: Eval("PP") %></td>
                                                <td class="text-center"><%#: Eval("DG") %></td>
                                                <td class="text-center fw-bold"><%#: Eval("PTS") %></td>
                                            </tr>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            </tbody>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                </table>
                            </div>
                            <asp:Panel ID="pnlPosicionesVacias" runat="server" CssClass="empty-state">
                                <i class="bi bi-bar-chart-line"></i>
                                <strong>Aun no hay posiciones publicadas</strong>
                                <span class="small text-secondary mt-1">Registra la temporada, inscribe los equipos y carga resultados para generar la tabla.</span>
                            </asp:Panel>
                        </div>
                        <div class="card-footer small text-secondary">Victoria: 3 puntos - Empate: 1 punto - Desempate: diferencia de goles y goles a favor.</div>
                    </div>
                </div>
                <div id="partidos" class="col-lg-4">
                    <div class="card card-primary card-outline">
                        <div class="card-header">
                            <h2 class="card-title">Partidos programados</h2>
                            <div class="card-tools">
                                <span class="badge text-bg-primary">Pendientes</span>
                            </div>
                        </div>
                        <div class="card-body">
                            <asp:Repeater ID="rptProximos" runat="server">
                                <ItemTemplate>
                                    <div class="border-bottom py-2">
                                        <div class="small text-secondary">Jornada <%#: Eval("Jornada") %> - <%#: Eval("Categoria") %> - <%#: Eval("FechaHora", "{0:dd/MM/yyyy HH:mm}") %></div>
                                        <div class="fw-semibold"><%#: Eval("EquipoLocal") %> vs <%#: Eval("EquipoVisita") %></div>
                                        <div class="small text-secondary"><%#: Eval("Cancha") %></div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="pnlProximosVacios" runat="server" CssClass="empty-state">
                                <i class="bi bi-calendar-event"></i>
                                <strong>Aun no hay partidos pendientes</strong>
                                <span class="small text-secondary mt-1">Los partidos programados apareceran aqui.</span>
                            </asp:Panel>
                        </div>
                    </div>
                    <div class="card card-secondary card-outline">
                        <div class="card-header">
                            <h2 class="card-title">Ultimos resultados</h2>
                        </div>
                        <div class="card-body">
                            <asp:Repeater ID="rptResultados" runat="server">
                                <ItemTemplate>
                                    <div class="border-bottom py-2">
                                        <div class="small text-secondary">Jornada <%#: Eval("Jornada") %> - <%#: Eval("Categoria") %></div>
                                        <div class="d-flex justify-content-between gap-2">
                                            <span><%#: Eval("EquipoLocal") %></span>
                                            <strong><%#: Eval("GolesLocal") %> - <%#: Eval("GolesVisita") %></strong>
                                            <span><%#: Eval("EquipoVisita") %></span>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="pnlResultadosVacios" runat="server" CssClass="empty-state">
                                <i class="bi bi-clipboard-data"></i>
                                <strong>Aun no hay resultados</strong>
                            </asp:Panel>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row g-3">
                <div class="col-12 col-xl-6">
                    <div class="card card-primary card-outline h-100">
                        <div class="card-header d-flex justify-content-between align-items-center">
                            <h2 class="card-title">Liga Mayor</h2>
                            <a href="Equipos.aspx" class="btn btn-sm btn-outline-primary">Ver equipos</a>
                        </div>
                        <div class="card-body">
                            <div class="row row-cols-2 row-cols-md-3 g-3">
                                <asp:Repeater ID="rptEquiposMayor" runat="server">
                                    <ItemTemplate>
                                        <div class="col"><article class="card h-100 mb-0"><div class="card-body text-center"><div class="club-crest"><img src='<%# ResolveUrl(Convert.ToString(Eval("LogoUrl"))) %>' alt='<%# System.Web.HttpUtility.HtmlAttributeEncode("Escudo " + Convert.ToString(Eval("Nombre"))) %>' loading="lazy" /></div><div class="small fw-semibold mt-2"><%#: Eval("Nombre") %></div></div></article></div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-12 col-xl-6">
                    <div class="card card-success card-outline h-100">
                        <div class="card-header d-flex justify-content-between align-items-center">
                            <h2 class="card-title">Liga Intermedia</h2>
                            <a href="Equipos.aspx" class="btn btn-sm btn-outline-success">Ver equipos</a>
                        </div>
                        <div class="card-body">
                            <div class="row row-cols-2 row-cols-md-3 g-3">
                                <asp:Repeater ID="rptEquiposIntermedia" runat="server">
                                    <ItemTemplate>
                                        <div class="col"><article class="card h-100 mb-0"><div class="card-body text-center"><div class="club-crest"><img src='<%# ResolveUrl(Convert.ToString(Eval("LogoUrl"))) %>' alt='<%# System.Web.HttpUtility.HtmlAttributeEncode("Escudo " + Convert.ToString(Eval("Nombre"))) %>' loading="lazy" /></div><div class="small fw-semibold mt-2"><%#: Eval("Nombre") %></div></div></article></div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="card bg-body-secondary">
                <div class="card-body d-flex flex-wrap justify-content-between align-items-center gap-3">
                    <div>
                        <h2 class="h5 mb-1">Campeones de la liga</h2>
                        <p class="text-secondary mb-0 small">Consulta el archivo historico de campeones por temporada y categoria.</p>
                    </div>
                    <a href="Campeones.aspx" class="btn btn-outline-success">Ver campeones</a>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
