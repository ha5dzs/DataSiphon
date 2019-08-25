function [xml_structure] = xml_string_to_struct(xml_string)
%XML_STRING_TO_STRUCT [xml_structure] = read_xml_string_to_struct(xml_string)
%   This function reads an XML string, and returns it as a structure based
%   on the children found. The data import process is not very neat, so you
%   will have to work on this further.
% Input argument is:
%   -> xml_string, which must be a string, and with valid XML syntax.
% Returns:
% -The XML structure.


    % This method was described on:
    % https://undocumentedmatlab.com/blog/parsing-xml-strings
    
    xml_string_object = java.io.StringBufferInputStream(xml_string);  % alternative #1
    % inputObject = org.xml.sax.InputSource(java.io.StringReader(xmlString));  % alternative #2

    xml_tree_object = xmlread(xml_string_object); % This feels weird,
    xml_structure = parseChildNodes(xml_tree_object); % But it works!


end

%% Internal functions.

% ----- Local function PARSECHILDNODES -----
function children = parseChildNodes(theNode)
% Recurse over node children.
% This function was taken from here:
% https://www.mathworks.com/help/matlab/ref/xmlread.html
    children = [];
    if theNode.hasChildNodes
       childNodes = theNode.getChildNodes;
       numChildNodes = childNodes.getLength;
       allocCell = cell(1, numChildNodes);

       children = struct(             ...
          'Name', allocCell, 'Attributes', allocCell,    ...
          'Data', allocCell, 'Children', allocCell);

        for count = 1:numChildNodes
            theChild = childNodes.item(count-1);
            children(count) = makeStructFromNode(theChild);
        end
    end
end

% ----- Local function MAKESTRUCTFROMNODE -----
function nodeStruct = makeStructFromNode(theNode)
% Create structure of node info.
% This function was taken from here:
% https://www.mathworks.com/help/matlab/ref/xmlread.html

    nodeStruct = struct(                        ...
       'Name', char(theNode.getNodeName),       ...
       'Attributes', parseAttributes(theNode),  ...
       'Data', '',                              ...
       'Children', parseChildNodes(theNode));

    if any(strcmp(methods(theNode), 'getData'))
       nodeStruct.Data = char(theNode.getData); 
    else
       nodeStruct.Data = '';
    end
end


% ----- Local function PARSEATTRIBUTES -----
function attributes = parseAttributes(theNode)
% Create attributes structure.
% This function was taken from here:
% https://www.mathworks.com/help/matlab/ref/xmlread.html

    attributes = [];
    if theNode.hasAttributes
       theAttributes = theNode.getAttributes;
       numAttributes = theAttributes.getLength;
       allocCell = cell(1, numAttributes);
       attributes = struct('Name', allocCell, 'Value', ...
                           allocCell);

       for count = 1:numAttributes
          attrib = theAttributes.item(count-1);
          attributes(count).Name = char(attrib.getName);
          attributes(count).Value = char(attrib.getValue);
       end
    end
end
