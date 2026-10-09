<%@ Page Title="Sanciones | Liga Arnaldo Chacon Soto" Language="C#" MasterPageFile="~/Vistas/Plantilla.Master" AutoEventWireup="true" CodeBehind="AdministrarSanciones.aspx.cs" Inherits="SistemaLigaChacon.Vistas.AdministrarSanciones" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <meta name="robots" content="noindex, nofollow" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="ContenedorPrincipal" runat="server">
    <div class="app-content-header">
        <div class="container-fluid">
            <h1 class="mb-1">Sanciones</h1>
            <p class="text-secondary mb-0">
                Registra tarjetas amarillas y rojas por partido.
            </p>
        </div>
    </div>

    <section class="content">
        <div class="container-fluid">
            <asp:Label ID="lblMensaje" runat="server"
                CssClass="alert alert-info d-block" Visible="false" />

            <div class="card card-primary card-outline">
                <div class="card-header">
                    <h2 class="card-title">Registrar tarjeta</h2>
                </div>
                <div class="card-body">
                    <div class="row g-3 align-items-end">
                        <div class="col-12 col-md-6">
                            <label class="form-label" for="ddlCompetencia">
                                Categoria y temporada
                            </label>
                            <asp:DropDownList ID="ddlCompetencia" runat="server"
                                CssClass="form-select" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlCompetencia_SelectedIndexChanged" />
                        </div>
                        <div class="col-12 col-md-6">
                            <label class="form-label" for="ddlPartido">Partido</label>
                            <asp:DropDownList ID="ddlPartido" runat="server"
                                CssClass="form-select" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlPartido_SelectedIndexChanged" />
                        </div>
                        <div class="col-12 col-md-4">
                            <label class="form-label" for="ddlEquipo">Equipo</label>
                            <asp:DropDownList ID="ddlEquipo" runat="server"
                                CssClass="form-select" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlEquipo_SelectedIndexChanged" />
                        </div>
                        <div class="col-12 col-md-4">
                            <label class="form-label" for="ddlJugador">Jugador</label>
                            <asp:DropDownList ID="ddlJugador" runat="server"
                                CssClass="form-select" AppendDataBoundItems="true">
                                <asp:ListItem Text="Selecciona un jugador" Value="" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-12 col-md-2">
                            <label class="form-label" for="ddlTipo">Tarjeta</label>
                            <asp:DropDownList ID="ddlTipo" runat="server"
                                CssClass="form-select">
                                <asp:ListItem Text="Amarilla" Value="AMARILLA" />
                                <asp:ListItem Text="Roja" Value="ROJA" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-12 col-md-2">
                            <label class="form-label" for="txtMinuto">Minuto</label>
                            <asp:TextBox ID="txtMinuto" runat="server"
                                CssClass="form-control" TextMode="Number"
                                min="0" max="120" />
                        </div>
                        <div class="col-12">
                            <label class="form-label" for="txtDescripcion">
                                Observacion (opcional)
                            </label>
                            <asp:TextBox ID="txtDescripcion" runat="server"
                                CssClass="form-control" MaxLength="500" />
                        </div>
                        <div class="col-12">
                            <asp:Panel ID="pnlSinJugadores" runat="server"
                                CssClass="alert alert-warning" Visible="false">
                                Este equipo no tiene jugadores inscritos. Primero agrega sus nombres en
                                <a href="AdministrarEquipos.aspx">Administrar equipos &gt; Plantilla del equipo</a>.
                            </asp:Panel>
                            <asp:Button ID="btnGuardar" runat="server"
                                Text="Guardar sancion"
                                CssClass="btn btn-primary"
                                OnClick="btnGuardar_Click" />
                        </div>
                    </div>
                </div>
            </div>

            <div class="card">
                <div class="card-header">
                    <h2 class="card-title">Tarjetas registradas</h2>
                </div>
                <div class="card-body table-responsive">
                    <asp:GridView ID="gvSanciones" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-striped align-middle"
                        GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="Jornada" HeaderText="Jornada" />
                            <asp:BoundField DataField="FechaHora" HeaderText="Fecha"
                                DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                            <asp:BoundField DataField="Partido" HeaderText="Partido" />
                            <asp:BoundField DataField="Equipo" HeaderText="Equipo" />
                            <asp:BoundField DataField="Jugador" HeaderText="Jugador" />
                            <asp:BoundField DataField="Tipo" HeaderText="Tarjeta" />
                            <asp:BoundField DataField="Minuto" HeaderText="Minuto" />
                            <asp:BoundField DataField="Descripcion" HeaderText="Observacion" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="alert alert-light border mb-0">
                                No hay tarjetas registradas para esta competencia.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
