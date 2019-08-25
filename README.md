# DataSiphon "library"

This library (that's a bit bold expression to this collection of code, but hey, why not? :)) allows a limited number of variables to be tossed from one environment to an other, with minimal conversion or compromise. The idea is that the user will interpret this as the same variable in a different environment, not just some clone. This can be great help when you have different live codes working on various horrible proprietary systems.

Originally I wanted to use this to interface three different systems that are not supposed to work together. I wanted to run the Python stuff as server so various proprietary environments can send small data packets to it. I decided to use XML for the data, so it allowed me to expand the formatting as I saw fit at the time.

### How does it work?

The project had the plug pulled before I got to do the networking, so it only works with string parsing. It should be very easy to convert the a payload of a packet into a string.  
Operation is as follows, irrespective of the programming language or environment:

* You create the XML header
* You add the variables you want in it
* You close the XML string, and send it, then...
* You receive the XML string, and either load all the variables from it, or you specifically extract the ones you care about, depending on what your environment allows you to do.

**WARNING**
This code allows your program to execute just about **ANYTHING** it receives in the form of plain text. It's not secure. It is intended to work in an isolated environment. Since it's very easily hackable please don't trust it.

## Environment-specifics

I added a ton of comments in the code, so please read that and understand it before you want to use it. If you are the tl;dr type, just read the following. 

The message format is something like this:
```
<dataFrame created_at=[Unix time stamp]>
	<[Variable Type] [Variable Arguments] variable_name=[Variable name inside environment]>
		[Variable content, programmatically generated]
	</[Variable Type]>
</dataFrame>
```
While it's important to stick to the order of the variable arguments, the variables themselves can be added in any order. This is because different XML parsers are being used and some are better than others.


### Matlab

* Creation example: `create_xml.m`
I just created the XML string like if it was plain text. The fancy function `xml_create_child_string()` detects the variable type and uses `sptrinf()`'s accordingly. It is expandable, so can work with variable types.  

* Reception example: `receive_xml.m`
This one uses Matlab's XML parser. On the string, it executes `xml_string_to_struct()`, which touches Matlab's internal Java stuff to do this. This was fun to write. Then the code calls `xml_get_matlab_code_from_struct()`, which creates the variable definition syntax from the XML structure, as if you wrote it in the editor. Then all you have to do is to execute each position in the string array with `eval()`. Nasty, but it works.

### Python

The Python implementation uses [`lxml.etree`](https://lxml.de/tutorial.html), and additional functions were written to make its use easier.  

* Manual creation example: `create_xml.py`
This one does manual detection and is a bit fiddly, but essentially it's an adaptation of a tutorial I wound on the [`lxml.etree`](https://lxml.de/tutorial.html) website. Afterwards, I convert the created object to a string, and save it.  

* Manual reception example: `receive_xml.py`
This one is also an [`lxml.etree`](https://lxml.de/tutorial.html) implementation.

Additionally, I have put together these as functions in `dataSiphon.py`, and the usage is shown in `dataSiphon_test.py`. All you have to do is to put your variables you want to send to a dict.


### C#

Now this one is a bit of a trainwreck. I have included the entire Visual Studio project. Works on Windows and it also works with Mono. The class and all the testing functions are included in the C# file.

* Creation
Mostly just simply creating strings. You create the header with `DataSiphon.BeginHeader()`, then add your variables with `DataSiphon.AddChild[Variable Type]()`, then you call `DataSiphon.EndHeader()`.  

* Reception
Since C# does not offer convenient solutions when you want to do pretty much anything, I had to work around a few things:  
	* Where possible I used `XmlReader`, but since it's single-direction, the order of variable arguments are important. I also used `XmlObject` on one occasion with `DataSiphon.FetchMatrix()`.
	* C# does not offer anything like 'eval', I think because the compilation and runtime model Microsoft uses. So, you must know the name and type of the variable in the XML string. Otherwise `DataSiphon.Fetch[Variable Type]()` will fail.
