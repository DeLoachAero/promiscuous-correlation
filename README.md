# promiscuous-correlation
An ASP.NET Web Api handler for accepting any sort of correlation ID header the caller dreams up.

Anyone creating web services that use other back-end services knows the value of correlation IDs
to help track a master request through the chain of child requests made by each link in the 
downstream chain.

The problem is that there is no standard of any kind for the HTTP header to use to hold the 
correlation ID.  X-Correlation-Id comes closer to "common use" than many options, but there is
still no guarantee that any given client will stick to that, especially in a complex cross-platform
environment found in many larger enterprises.

Most correlation ID handlers you find tend to implement their personal favorite header and ignore
any others, limiting cross-platform use.

This correlation ID handler, however, is promiscuous -- it's willing to correlate with anyone!  :)

It has a built-in list (that you can extend) which holds the most common headers seen in the real
world for accepting incoming correlation IDs and includes the option of RegEx patterns to
locate the header(s) as well.  

Extensions to HttpRequestMessage let you easily retrieve the ID that was sent
(no matter what header it came from), and also to get a list of headers and values that you
should place onto any requests your service makes to any other service to pass the header(s)
along downstream. 

Be default, the handler accepts a number of potential headers, but provides an outgoing list for 
sending in downstream requests or in the HttpResponseMessage that includes only X-Correlation-Id,
plus whatever header the caller originally used.

Also by default, if it must generate a brand new ID (when no header was found) it generates an ID
in the form of a GUID.  These defaults can be easily overridden as needed.

In a sane world, none of this would be necessary. Sadly...

