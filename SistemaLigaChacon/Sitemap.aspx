<%@ Page Language="C#" ContentType="application/xml; charset=utf-8" %><%
    var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/");
    var pages = new[] { "Vistas/Home.aspx", "Vistas/Equipos.aspx", "Vistas/Posiciones.aspx", "Vistas/Campeones.aspx" };
    Response.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    Response.Write("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
    foreach (var page in pages)
    {
        var url = baseUrl.TrimEnd('/') + "/" + page;
        Response.Write("<url><loc>" + Server.HtmlEncode(url) + "</loc></url>");
    }
    Response.Write("</urlset>");
%>
