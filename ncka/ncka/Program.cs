using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Nunjsharp;

namespace ncka
{
    class Program
    {
        class WondersItem
        {
            public string Wonders;

            public WondersItem()
            {
                Wonders = "Gerson";
            }
        };

        class OfItem
        {
            public WondersItem Of = new WondersItem();
        };


        class RootItem
        {
            public readonly OfItem World = new OfItem();
        };


        static void Main(string[] args)
        {
            
            var tl = new Nunjsharp.Environment();

            tl.ParseTemplate("parent.html", @"{% block header %}
This is the default content
{% endblock %}
{# Loop through all the users #}
<section class=""left"">
  {% block left %}{% endblock %}
</section>
{{ World[""Of""][""Wonders""] }}
<section class=""right"">
{% block right %}  This is more content {% endblock %}
</section>
");
            tl.ParseTemplate("extends.html", @"{% extends ""parent.html"" %}

{% block left %}            This is the left side!{% endblock %}

{% block right %}{{ super() }}This is the right side!{% endblock %}
");

            var data = new RootItem();

            Trace.TraceInformation(tl.RenderTemplate("extends.html", data));
        }
    }
}
