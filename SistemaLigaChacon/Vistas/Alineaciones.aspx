<%@ Page Title="Alineaciones | Liga Arnaldo Chacon Soto" Language="C#" MasterPageFile="~/Vistas/Plantilla.Master" AutoEventWireup="true" CodeBehind="Alineaciones.aspx.cs" Inherits="SistemaLigaChacon.Vistas.Alineaciones" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <meta name="robots" content="noindex, nofollow" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="ContenedorPrincipal" runat="server">
    <div class="app-content-header">
        <div class="container-fluid">
            <h1 class="mb-1">Alineaciones</h1>
            <p class="text-secondary mb-0">
                La titularidad y la suplencia se definen para cada partido.
            </p>
        </div>
    </div>

    <section class="content">
        <div class="container-fluid">
            <asp:Label ID="lblMensaje" runat="server"
                CssClass="alert alert-info d-block" Visible="false" />

            <div class="card">
                <div class="card-header">
                    <h2 class="card-title">Elegir partido y equipo</h2>
                </div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-12 col-md-4">
                            <label class="form-label" for="ddlCompetencia">
                                Categoria y temporada
                            </label>
                            <asp:DropDownList ID="ddlCompetencia" runat="server"
                                CssClass="form-select" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlCompetencia_SelectedIndexChanged" />
                        </div>
                        <div class="col-12 col-md-5">
                            <label class="form-label" for="ddlPartido">Partido</label>
                            <asp:DropDownList ID="ddlPartido" runat="server"
                                CssClass="form-select" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlPartido_SelectedIndexChanged" />
                        </div>
                        <div class="col-12 col-md-3">
                            <label class="form-label" for="ddlEquipo">Equipo</label>
                            <asp:DropDownList ID="ddlEquipo" runat="server"
                                CssClass="form-select" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlEquipo_SelectedIndexChanged" />
                        </div>
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnlAdministracion" runat="server" Visible="false">
                <div class="card card-primary card-outline">
                    <div class="card-header">
                        <h2 class="card-title">Asignar jugador</h2>
                    </div>
                    <div class="card-body">
                        <p class="text-secondary">
                            Agrega a cada futbolista y marca si inicia o espera como suplente.
                            Un equipo puede tener hasta 11 titulares por partido.
                        </p>
                        <div class="row g-3 align-items-end">
                            <div class="col-12 col-md-4">
                                <label class="form-label" for="ddlJugador">Jugador</label>
                                <asp:DropDownList ID="ddlJugador" runat="server"
                                    CssClass="form-select" />
                            </div>
                            <div class="col-12 col-md-3">
                                <label class="form-label" for="ddlTipo">Participacion</label>
                                <asp:DropDownList ID="ddlTipo" runat="server"
                                    CssClass="form-select">
                                    <asp:ListItem Text="Titular" Value="TITULAR" />
                                    <asp:ListItem Text="Suplente" Value="SUPLENTE" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-12 col-md-3">
                                <label class="form-label" for="txtPosicion">
                                    Posicion (opcional)
                                </label>
                                <asp:TextBox ID="txtPosicion" runat="server"
                                    CssClass="form-control" MaxLength="30"
                                    placeholder="Portero, defensa, delantero..." />
                            </div>
                            <div class="col-12 col-md-2">
                                <div class="form-check mb-2">
                                    <asp:CheckBox ID="chkEsPortero" runat="server"
                                        CssClass="form-check-input" />
                                    <label class="form-check-label" for="chkEsPortero">
                                        Portero
                                    </label>
                                </div>
                            </div>
                            <div class="col-12">
                                <asp:Button ID="btnGuardar" runat="server"
                                    Text="Guardar jugador en alineacion"
                                    CssClass="btn btn-primary"
                                    OnClick="btnGuardar_Click" />
                            </div>
                        </div>
                        <p class="small mt-3 mb-0">
                            Para agregar jugadores que no aparecen,
                            ve a <a href="AdministrarEquipos.aspx">Administrar equipos &gt; Plantilla del equipo</a>.
                        </p>
                    </div>
                </div>
            </asp:Panel>

            <div class="card card-success card-outline">
                <div class="card-header">
                    <h2 class="card-title">Alineacion del equipo</h2>
                </div>
                <div class="card-body table-responsive">
                    <asp:GridView ID="gvAlineacion" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-striped align-middle"
                        GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="Tipo" HeaderText="Titular / suplente" />
                            <asp:BoundField DataField="Dorsal" HeaderText="Dorsal" />
                            <asp:BoundField DataField="Nombre" HeaderText="Jugador" />
                            <asp:BoundField DataField="Posicion" HeaderText="Posicion" />
                            <asp:CheckBoxField DataField="EsPortero" HeaderText="Portero" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="alert alert-light border mb-0">
                                Aun no se ha publicado la alineacion para este equipo y partido.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
