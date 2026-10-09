<%@ Page Title="Administrar partidos | Liga Arnaldo Chacon Soto" Language="C#" MasterPageFile="~/Vistas/Plantilla.Master" AutoEventWireup="true" CodeBehind="AdministrarPartidos.aspx.cs" Inherits="SistemaLigaChacon.Vistas.AdministrarPartidos" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <meta name="robots" content="noindex, nofollow" />
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="ContenedorPrincipal" runat="server">
    <div class="app-content-header">
        <div class="container-fluid">
            <h1 class="mb-1">Partidos y resultados</h1>
            <p class="text-secondary mb-0">Programa partidos y guarda sus marcadores. La tabla se calcula al guardar el resultado.</p>
        </div>
    </div>
    <section class="content">
        <div class="container-fluid">
            <asp:Label ID="lblMensaje" runat="server" CssClass="alert alert-info d-block" Visible="false" />
            <div class="card card-primary card-outline">
                <div class="card-header">
                    <h2 class="card-title">
                        <asp:Literal ID="litModoFormulario" runat="server" Text="Programar partido" /></h2>
                </div>
                <div class="card-body">
                    <div class="row g-3 align-items-end">
                        <div class="col-12 col-md-3">
                            <label class="form-label" for="ddlCompetencia">Categoria y temporada</label>
                            <asp:DropDownList ID="ddlCompetencia" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCompetencia_SelectedIndexChanged" />
                        </div>
                        <div class="col-6 col-md-2">
                            <label class="form-label" for="ddlJornada">Jornada</label>
                            <asp:DropDownList ID="ddlJornada" runat="server" CssClass="form-select" />
                        </div>
                        <div class="col-6 col-md-2">
                            <label class="form-label" for="txtFecha">Fecha y hora</label>
                            <asp:TextBox ID="txtFecha" runat="server" TextMode="DateTimeLocal" CssClass="form-control" />
                        </div>
                        <div class="col-12 col-md-2">
                            <label class="form-label" for="ddlLocal">Equipo local</label>
                            <asp:DropDownList ID="ddlLocal" runat="server" CssClass="form-select" />
                        </div>
                        <div class="col-12 col-md-2">
                            <label class="form-label" for="ddlVisita">Equipo visitante</label>
                            <asp:DropDownList ID="ddlVisita" runat="server" CssClass="form-select" />
                        </div>
                        <div class="col-12 col-md-2">
                            <label class="form-label" for="ddlCancha">Cancha</label>
                            <asp:DropDownList ID="ddlCancha" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Selecciona cancha" Value="" />
                                <asp:ListItem Text="Estadio Alex Pineda Chacon" Value="Estadio Alex Pineda Chacon" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-12">
                            <asp:HiddenField ID="hfPartidoEditarId" runat="server" />
                            <asp:HiddenField ID="hfVersionEdicion" runat="server" />
                            <asp:Button ID="btnCrearPartido" runat="server" Text="Guardar partido" CssClass="btn btn-primary" OnClick="btnCrearPartido_Click" />
                            <asp:Button ID="btnActualizarPartido" runat="server" Text="Guardar cambios" CssClass="btn btn-warning" OnClick="btnActualizarPartido_Click" Visible="false" />
                            <asp:Button ID="btnCancelarEdicion" runat="server" Text="Cancelar edicion" CssClass="btn btn-outline-secondary ms-2" OnClick="btnCancelarEdicion_Click" CausesValidation="false" Visible="false" />
                        </div>
                    </div>
                </div>
            </div>
            <div class="card">
                <div class="card-header">
                    <h2 class="card-title">Calendario y carga de marcadores</h2>
                </div>
                <div class="card-body table-responsive">
                    <asp:GridView ID="gvPartidos" runat="server" AutoGenerateColumns="false" CssClass="table table-striped align-middle" GridLines="None" DataKeyNames="PartidoId" OnRowCommand="gvPartidos_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="Jornada" HeaderText="Jornada" />
                            <asp:BoundField DataField="FechaHora" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                            <asp:BoundField DataField="EquipoLocal" HeaderText="Local" />
                            <asp:BoundField DataField="EquipoVisita" HeaderText="Visitante" />
                            <asp:BoundField DataField="Cancha" HeaderText="Cancha" />
                            <asp:BoundField DataField="Estado" HeaderText="Estado" />
                            <asp:TemplateField HeaderText="Marcador">
                                <ItemTemplate>
                                    <div class="d-flex align-items-center gap-2">
                                        <asp:TextBox ID="txtGolesLocal" runat="server" CssClass="form-control" Text='<%# Convert.ToString(Eval("GolesLocal")) %>' MaxLength="2" />
                                        <span>:</span>
                                        <asp:TextBox ID="txtGolesVisita" runat="server" CssClass="form-control" Text='<%# Convert.ToString(Eval("GolesVisita")) %>' MaxLength="2" />
                                        <asp:HiddenField ID="hfVersionFila" runat="server" Value='<%# Convert.ToBase64String((byte[])Eval("VersionFila")) %>' />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Accion">
                                <ItemTemplate>
                                    <div class="d-flex flex-column gap-1">
                                        <asp:Button ID="btnGuardarResultado" runat="server" Text="Guardar resultado" CssClass="btn btn-sm btn-success text-nowrap" CommandName="GuardarResultado" CommandArgument='<%# Eval("PartidoId") %>' CausesValidation="false" />
                                        <asp:Button ID="btnEditarProgramacion" runat="server" Text="Editar" CssClass="btn btn-sm btn-outline-primary" CommandName="EditarProgramacion" CommandArgument='<%# Eval("PartidoId") %>' CausesValidation="false" Visible='<%# Convert.ToString(Eval("Estado")) == "PENDIENTE" %>' />
                                        <asp:Button ID="btnEliminarPartido" runat="server" Text="Eliminar" CssClass="btn btn-sm btn-outline-danger" CommandName="EliminarPartido" CommandArgument='<%# Eval("PartidoId") %>' CausesValidation="false" OnClientClick="return confirm('Eliminar este partido pendiente?');" Visible='<%# Convert.ToString(Eval("Estado")) == "PENDIENTE" %>' />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="alert alert-light border mb-0">Todavia no hay partidos programados para esta competencia.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
