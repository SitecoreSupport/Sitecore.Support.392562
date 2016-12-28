using HtmlAgilityPack;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Links;
using Sitecore.Web;

namespace Sitecore.Support.Data.Fields
{
  public class HtmlField : Sitecore.Data.Fields.HtmlField
  {
    public HtmlField(Field innerField) : base(innerField)
    {
    }

    private ID GetLinkedID(string href)
    {
      DynamicLink link;
      Assert.ArgumentNotNull(href, "href");
      try
      {
        link = DynamicLink.Parse(href);
      }
      catch (InvalidLinkFormatException)
      {
        return null;
      }
      return link.ItemId;
    }

    public override void RemoveLink(ItemLink itemLink)
    {
      Assert.ArgumentNotNull(itemLink, "itemLink");
      if (!InnerField.HasValue) return;
      var document = new HtmlDocument();
      document.LoadHtml(Value);

      if (!(RemoveTextLinks(itemLink, document) | RemoveMediaLinks(itemLink, document))) return;
      RuntimeHtml.FixBullets(document);
      RuntimeHtml.FixSelectOptions(document);
      Value = document.DocumentNode.OuterHtml;
    }

    private bool RemoveMediaLinks(ItemLink itemLink, HtmlDocument document)
    {
      Assert.ArgumentNotNull(itemLink, "itemLink");
      Assert.ArgumentNotNull(document, "document");
      var nodes = document.DocumentNode.SelectNodes("//img");
      if (nodes == null) return false;
      var flag = false;

      foreach (var node in nodes)
      {
        var attributeValue = node.GetAttributeValue("src", string.Empty);
        if (string.IsNullOrEmpty(attributeValue)) continue;
        var linkedID = GetLinkedID(attributeValue);
        if ((linkedID == (ID) null) || (linkedID != itemLink.TargetItemID)) continue;
        node.ParentNode.RemoveChild(node, true);
        flag = true;
      }
      return flag;
    }

    private bool RemoveTextLinks(ItemLink itemLink, HtmlDocument document)
    {
      Assert.ArgumentNotNull(itemLink, "itemLink");
      Assert.ArgumentNotNull(document, "document");
      var nodes = document.DocumentNode.SelectNodes("//a[@href]");
      if (nodes == null) return false;
      var flag = false;

      foreach (var node in nodes)
      {
        var attributeValue = node.GetAttributeValue("href", string.Empty);
        if (string.IsNullOrEmpty(attributeValue)) continue;
        var linkedID = GetLinkedID(attributeValue);

        if ((linkedID == (ID) null) || linkedID != itemLink.TargetItemID) continue;
        node.ParentNode.RemoveChild(node, true);
        flag = true;
      }
      return flag;
    }
  }
}