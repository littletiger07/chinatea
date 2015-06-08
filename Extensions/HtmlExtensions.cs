using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChinaTea.Models;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ChinaTea.Entities;

namespace ChinaTea.Extensions
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString Paging(this HtmlHelper htmlHelper, PageInfo pageInfo)
        {

            int start = ((pageInfo.CurrentPage - 1) / 10) * 10 + 1;
            int end;
            if (pageInfo.PageCount < (start + 9))
            {
                end = pageInfo.PageCount;
            }
            else
            {
                end = start + 9;
            }
            TagBuilder divBuilder = new TagBuilder("div");
            divBuilder.MergeAttribute("class", "pagination_div");
            TagBuilder spanBuilder = new TagBuilder("span");
            spanBuilder.GenerateId("pagenav");
            TagBuilder firstPageHrefBuilder = new TagBuilder("a");
            if (pageInfo.CurrentPage != 1)
            {
                firstPageHrefBuilder.MergeAttribute("href", pageInfo.Url + "1");
            }
            firstPageHrefBuilder.InnerHtml = "|<<";
            spanBuilder.InnerHtml += firstPageHrefBuilder;
            TagBuilder ahrefBuilder = new TagBuilder("a");

            if (pageInfo.CurrentPage > 1)
            {
                ahrefBuilder.MergeAttribute("href", pageInfo.Url + (pageInfo.CurrentPage - 1).ToString());
            }
            ahrefBuilder.InnerHtml = "prev";
            spanBuilder.InnerHtml += ahrefBuilder;
            for (int i = start; i <= end; i++)
            {
                TagBuilder aBuilder = new TagBuilder("a");
                if (i != pageInfo.CurrentPage)
                {
                    aBuilder.MergeAttribute("href", pageInfo.Url + i);
                }
                else
                {
                    aBuilder.MergeAttribute("class", "pagecurrent");
                }
                aBuilder.InnerHtml = i.ToString();
                spanBuilder.InnerHtml += aBuilder;
            }
            TagBuilder ahrefBuilder1 = new TagBuilder("a");

            if (pageInfo.CurrentPage < pageInfo.PageCount)
            {
                ahrefBuilder1.MergeAttribute("href", pageInfo.Url + (pageInfo.CurrentPage + 1).ToString());
            }
            ahrefBuilder1.InnerHtml = "next";
            spanBuilder.InnerHtml += ahrefBuilder1;
            TagBuilder lastPageHrefBuilder = new TagBuilder("a");
            if (pageInfo.CurrentPage != pageInfo.PageCount)
            {
                lastPageHrefBuilder.MergeAttribute("href", pageInfo.Url + pageInfo.PageCount.ToString());
            }
            lastPageHrefBuilder.InnerHtml = ">>|";
            spanBuilder.InnerHtml += lastPageHrefBuilder;
            TagBuilder spanBuilder1 = new TagBuilder("span");
            spanBuilder1.GenerateId("pagejumper_span");
            spanBuilder1.InnerHtml = "Page: ";
            TagBuilder selectBuilder = new TagBuilder("select");
            selectBuilder.GenerateId("pagejumper_select");
            for (int i = 1; i <= pageInfo.PageCount; i++)
            {
                TagBuilder optionBuilder = new TagBuilder("option");
                optionBuilder.MergeAttribute("value", i.ToString());
                if (i == pageInfo.CurrentPage)
                {
                    optionBuilder.MergeAttribute("selected", "selected");
                }
                optionBuilder.InnerHtml = i.ToString();
                selectBuilder.InnerHtml += optionBuilder;
            }
            spanBuilder1.InnerHtml += selectBuilder;
            spanBuilder1.InnerHtml += (" of " + pageInfo.PageCount.ToString());
            divBuilder.InnerHtml += spanBuilder;
            divBuilder.InnerHtml += spanBuilder1;
            TagBuilder jsBuilder = new TagBuilder("script");
            jsBuilder.InnerHtml = "$(document).ready(function(){$('#pagejumper_select').change(function(){var pageToJump=$('#pagejumper_select').val();url='" + pageInfo.Url + "'+pageToJump+''; window.location=url})})";
            divBuilder.InnerHtml += jsBuilder;
            return MvcHtmlString.Create(divBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString Pagination(this HtmlHelper htmlHelper, PageInfo pageInfo)
        {
            int start = ((pageInfo.CurrentPage - 1) / 10) * 10 + 1;
            int end;
            if (pageInfo.PageCount < (start + 9))
            {
                end = pageInfo.PageCount;
            }
            else
            {
                end = start + 9;
            }
            TagBuilder allDivBuilder = new TagBuilder("div");
            TagBuilder pagingDivBuilder = new TagBuilder("div");
            pagingDivBuilder.MergeAttribute("class", "col-md-8");
            TagBuilder olBuilder = new TagBuilder("ol");
            olBuilder.MergeAttribute("class", "pagination pagination-lg");
            TagBuilder liBuilder = new TagBuilder("li");
            TagBuilder ahrefBuilder = new TagBuilder("a");
            if (pageInfo.CurrentPage > 1)
            {
                ahrefBuilder.MergeAttribute("href", pageInfo.Url + (pageInfo.CurrentPage - 1).ToString());
            }
            ahrefBuilder.InnerHtml = "&laquo;prev";
            liBuilder.InnerHtml += ahrefBuilder;
            olBuilder.InnerHtml += liBuilder;
            for (int i = start; i <= end; i++)
            {
                liBuilder=new TagBuilder("li");
                TagBuilder aBuilder = new TagBuilder("a");
                aBuilder.MergeAttribute("href", pageInfo.Url + i);
                if (i == pageInfo.CurrentPage)
                {
                    liBuilder.MergeAttribute("class", "active");
                }

                aBuilder.InnerHtml = i.ToString();
                liBuilder.InnerHtml += aBuilder;
                olBuilder.InnerHtml+=liBuilder;
            }
            ahrefBuilder = new TagBuilder("a");
            liBuilder = new TagBuilder("li");
            if (pageInfo.CurrentPage < pageInfo.PageCount)
            {
                ahrefBuilder.MergeAttribute("href", pageInfo.Url + (pageInfo.CurrentPage + 1).ToString());
            }
            ahrefBuilder.InnerHtml = "next&raquo;";
            liBuilder.InnerHtml += ahrefBuilder;
            olBuilder.InnerHtml += liBuilder;
            pagingDivBuilder.InnerHtml += olBuilder;
            allDivBuilder.InnerHtml += pagingDivBuilder;
            TagBuilder jsDivBuilder = new TagBuilder("div");
            jsDivBuilder.MergeAttribute("class", "col-md-offset-0 col-md-4 text-primary");
            jsDivBuilder.InnerHtml = "Page: ";
            TagBuilder selectBuilder = new TagBuilder("select");
            selectBuilder.GenerateId("pagejumper_select");
            for (int i = 1; i <= pageInfo.PageCount; i++)
            {
                TagBuilder optionBuilder = new TagBuilder("option");
                optionBuilder.MergeAttribute("value", i.ToString());
                if (i == pageInfo.CurrentPage)
                {
                    optionBuilder.MergeAttribute("selected", "selected");
                    
                }
                optionBuilder.MergeAttribute("class", "text-primary");
                optionBuilder.InnerHtml = i.ToString();
                selectBuilder.InnerHtml += optionBuilder;
            }
            jsDivBuilder.InnerHtml += selectBuilder;
            jsDivBuilder.InnerHtml += (" of " + pageInfo.PageCount.ToString());
            allDivBuilder.InnerHtml += jsDivBuilder;
            TagBuilder jsBuilder = new TagBuilder("script");
            jsBuilder.InnerHtml = "$(document).ready(function(){$('#pagejumper_select').change(function(){var pageToJump=$('#pagejumper_select').val();url='" + pageInfo.Url + "'+pageToJump+''; window.location=url})})";
            allDivBuilder.InnerHtml += jsBuilder;
            return MvcHtmlString.Create(allDivBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString SelectQuantities(this HtmlHelper htmlHelper, string name,int n,int selected=1)
        {
            TagBuilder selectBuilder = new TagBuilder("select");
            selectBuilder.MergeAttribute("name", name);
            for (int i=1; i<=n; i++)
            {
                TagBuilder optionBuilder = new TagBuilder("option");
                optionBuilder.MergeAttribute("value", i.ToString());
                if (i==selected)
                {
                    optionBuilder.MergeAttribute("selected", "selected");
                }
                optionBuilder.InnerHtml = i.ToString();
                selectBuilder.InnerHtml += optionBuilder;
            }
            return MvcHtmlString.Create(selectBuilder.ToString(TagRenderMode.Normal));
        }
        public static MvcHtmlString ProductCategorySelectList(this HtmlHelper htmlHelper, int selectedId=-1)
        {
            ChinaTeaEntities dbContext = new ChinaTeaEntities();
            IEnumerable<ProductCategory> categories = dbContext.ProductCategories;
            TagBuilder selectBuilder = new TagBuilder("select");
            selectBuilder.MergeAttribute("name", "ProductCategoryId");
            foreach(var category in categories)
            {
                TagBuilder optionBuilder = new TagBuilder("option");
                optionBuilder.MergeAttribute("value", category.Id.ToString());
                if (category.Id == selectedId)
                {
                    optionBuilder.MergeAttribute("selected", "selected");
                }
                optionBuilder.InnerHtml = category.ProductCategoryName;
                selectBuilder.InnerHtml += optionBuilder;
            }
            return MvcHtmlString.Create(selectBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString ReplaceNewLines(this HtmlHelper htmlHelper, MvcHtmlString str)
        {
            string pattern = @"\r\n|\n\r|\r|\n";
            string  htmlStr = str.ToHtmlString();
            string result = Regex.Replace(htmlStr, pattern, " ");
            StringBuilder builder = new StringBuilder();
            builder.Append(result);
            return MvcHtmlString.Create(builder.ToString());


        }

        
    }
}