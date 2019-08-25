% This code generates an xml string out of some variables I have.
clear all;
clc;

matlab_triplet = [121.545, 1645, 120.110]; % This is a test triplet.
matlab_matrix = [
    1, 2, 3, 5;
    453, 34342, 342, 10.11;
    3432.12, 3.43534, 564.3452, 0;
    0, 0, 0, 1;
    ];
matlab_string = 'Woohoo. This was sent from Matlab.'; % test string.
matlab_boolean = true;
matlab_single_variable_as_matrix = 42;

time_now = posixtime(datetime(datetime, 'TimeZone', 'UTC')); % Get time in UTC, apparently.
%% Let's assemble the xml in an xml node!
% manually joining strings are faster.
tic
% Create the root.
output_xml_string = sprintf('<dataFrame created_at="%0.6f">\n', time_now);

output_xml_string = [output_xml_string, xml_create_child_string(matlab_single_variable_as_matrix), xml_create_child_string(matlab_triplet), xml_create_child_string(matlab_matrix), xml_create_child_string(matlab_string), xml_create_child_string(matlab_boolean)];

output_xml_string = [output_xml_string, sprintf('</dataFrame>\n')];
% Finish the thing off.
time_taken = toc

% it to a file.
h = fopen('matlab_generated.xml', 'w');
fprintf(h, output_xml_string);
fclose(h);
