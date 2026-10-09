<%@ Page Title="Posiciones | Liga Arnaldo Chacon Soto" MetaDescription="Consulta la tabla de posiciones de la Liga Arnaldo Chacon Soto, con puntos, partidos jugados, goles y clasificacion por categoria." Language="C#" MasterPageFile="~/Vistas/Plantilla.Master" AutoEventWireup="true" CodeBehind="Posiciones.aspx.cs" Inherits="SistemaLigaChacon.Vistas.Posiciones" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <style>
        .standings-empty {
            min-height: 200px;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            text-align: center
        }

            .standings-empty .empty-icon {
                font-size: 2rem;
                color: #83928a
            }

            .standings-empty p {
                max-width: 420px
            }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="ContenedorPrincipal" runat="server">
    <div class="app-content-header">
        <div class="container-fluid">
            <div class="row">
                <div class="col-sm-8">
                    <h1 class="mb-1">Tabla de posiciones</h1>
                    <p class="text-secondary mb-0">Clasificacion del torneo por categoria.</p>
                </div>
                <div class="col-sm-4">
                    <ol class="breadcrumb float-sm-end">
                        <li class="breadcrumb-item">
                            <a href="Home.aspx">Inicio</a>
                        </li>
                        <li class="breadcrumb-item active">Posiciones</li>
                    </ol>
                </div>
            </div>
        </div>
    </div>
    <section class="content">
        <div class="container-fluid">
            <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false" />
            <div class="card card-success card-outline">
                <div class="card-header d-flex flex-wrap align-items-center justify-content-between gap-2">
                    <h2 class="card-title">Categoria
                        <asp:Literal ID="litCategoria" runat="server" />
                    </h2>
                    <div class="btn-group" role="group" aria-label="Seleccionar categoria">
                        <asp:HyperLink ID="lnkMayor" runat="server" CssClass="btn btn-outline-success" Text="Mayor" />
                        <asp:HyperLink ID="lnkIntermedia" runat="server" CssClass="btn btn-outline-success" Text="Intermedia" />
                    </div>
                </div>
                <div class="card-body p-0">
                    <div class="table-responsive">
                        <table class="table table-striped table-hover align-middle mb-0" aria-label="Tabla de posiciones">
                            <thead class="table-light">
                                <tr>
                                    <th>#</th>
                                    <th>Equipo</th>
                                    <th class="text-center">PJ</th>
                                    <th class="text-center">PG</th>
                                    <th class="text-center">PE</th>
                                    <th class="text-center">PP</th>
                                    <th class="text-center">GF</th>
                                    <th class="text-center">GC</th>
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
                                        <td class="text-center"><%#: Eval("GF") %></td>
                                        <td class="text-center"><%#: Eval("GC") %></td>
                                        <td class="text-center"><%#: Eval("DG") %></td>
                                        <td class="text-center fw-bold"><%#: Eval("PTS") %></td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate></tbody></FooterTemplate>
                            </asp:Repeater>
                        </table>
                    </div>
                    <asp:Panel ID="pnlVacio" runat="server" CssClass="standings-empty">
                        <div class="empty-icon">▤</div>
                        <strong>Aun no hay posiciones publicadas</strong>
                        <p class="text-secondary mb-0 mt-1">Crea la temporada y la competencia, inscribe los equipos y registra resultados para completar esta tabla.</p>
                    </asp:Panel>
                </div>
                <div class="card-footer text-secondary small">
                    <strong>Puntuacion:</strong> victoria 3 puntos, empate 1, derrota 0. Desempate: diferencia de goles y luego goles a favor.
                </div>
            </div>
        </div>
    </section>
</asp:Content>
