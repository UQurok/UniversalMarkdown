﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalMarkdown.Helpers;

namespace UniversalMarkdown.Parse.Elements
{
    class MarkdownLinkInline : MarkdownInline
    {
        string Url { get; set; }

        public MarkdownLinkInline()
            : base(MarkdownInlineType.MarkdownLink)
        { }

        internal override int Parse(ref string markdown, int startingPos, int endingPos)
        {
            // Find all of the link parts
            int linkTextOpen = Common.IndexOf(ref markdown, '[', startingPos, endingPos);
            int linkTextClose = Common.IndexOf(ref markdown, ']', linkTextOpen, endingPos);
            int linkOpen = Common.IndexOf(ref markdown, '(', linkTextClose, endingPos);
            int linkClose = Common.IndexOf(ref markdown, ')', linkOpen, endingPos);

            // These should always be =
            if (linkTextOpen != startingPos)
            {
                DebuggingReporter.ReportCriticalError("link parse didn't find [ in at the starting pos");
            }            
            if (linkClose + 1 != endingPos)
            {
                DebuggingReporter.ReportCriticalError("link parse didn't find ] in at the end pos");
            }

            // Make sure there is something to parse, and not just dead space
            linkTextOpen++;
            if (linkTextClose > linkTextOpen)
            {
                // Parse any children of this link element
                ParseInlineChildren(ref markdown, linkTextOpen, linkTextClose);
            }
            
            // Grab the link
            linkOpen++;
            Url = markdown.Substring(linkOpen, linkClose - linkOpen);         

            // Return the point after the )
            return linkClose + 1;
        }

        /// <summary>
        /// Attempts to find a element in the range given. If an element is found we must check if the starting is less than currentNextElementStart,
        /// and if so update that value to be the start and update the elementEndPos to be the end of the element. These two vales will be passed back to us
        /// when we are asked to parse. We then return true or false to indicate if we are the new candidate. 
        /// </summary>
        /// <param name="markdown">mark down to parse</param>
        /// <param name="currentPos">the starting point to search</param>
        /// <param name="maxEndingPos">the ending point to search</param>
        /// <param name="elementStartingPos">the current starting element, if this element is < we will update this to be our starting pos</param>
        /// <param name="elementEndingPos">The ending pos of this element if it is interesting.</param>
        /// <returns>true if we are the next element candidate, false otherwise.</returns>
        public static bool FindNextClosest(ref string markdown, int startingPos, int endingPos, ref int currentNextElementStart, ref int elementEndingPos)
        {
            // Test for links
            int linkTextOpen = Common.IndexOf(ref markdown, '[', startingPos, endingPos);
            if (linkTextOpen != -1 && linkTextOpen < currentNextElementStart)
            {
                // Ensure we have a link
                int linkTextClose = Common.IndexOf(ref markdown, ']', linkTextOpen, endingPos);
                if (linkTextClose != -1)
                {
                    int linkOpen = Common.IndexOf(ref markdown, '(', linkTextClose, endingPos);
                    if (linkOpen != -1)
                    {
                        int linkClose = Common.IndexOf(ref markdown, ')', linkOpen, endingPos);
                        if (linkClose != -1)
                        {
                            // Make sure the order is correct
                            if (linkTextOpen < linkTextClose && linkTextClose < linkOpen && linkOpen < linkClose)
                            {
                                currentNextElementStart = linkTextOpen;
                                elementEndingPos = linkClose + 1;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}