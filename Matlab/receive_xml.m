% This script parses the XML file, and retrieves data.
clear all
clc

% We need to be a bit innovative here. Let's read in the xml file, and
% store it as a string in the memory. This is needed because we will
% receive xml strings in UDP packets.




%xml_file = 'python_generated.xml';
%xml_file = 'matlab_generated.xml';
xml_file = 'csharp_generated.xml';
xml_string = fileread(xml_file);

xml_struct = xml_string_to_struct(xml_string);
% and now we need to manually get the stuff. Guess this will be some sort
% of a function.

tic;
variable_definitions = xml_get_matlab_code_from_struct(xml_struct);

% and now we load these variables to the current workspace.
for(i = 1:length(variable_definitions))
    eval(variable_definitions{i})
end

time_taken = round(toc, 3) % In milliseconds.