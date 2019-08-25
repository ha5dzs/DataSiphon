function [output_string] = xml_create_child_string(input_variable, varargin)
%XML_CREATE_CHILD_STRING [output_string] = xml_create_child_string(input_variable, [variable_name])
%   This function creates an xml child string from a variable, whihc can be
%   pasted together to form an xml document. The variable type is
%   automatically detected.
% Input argument:
%   -> input_variable, which may be a triplet, a matrix, a string, or a
%      boolean.
%   -> variable_name is an optional input argument. If you toss a 
% Returns:
% -output_string, which is to be pasted in the document.

    %% Do some sanity checks.
    if(isstruct(input_variable))
        % Reject structures.
        error('This function doesn''t work with structures.')
    end

    if(length(size(input_variable)) > 2)
        % Reject arrays with more than two dimensions
        error('This function can only handle matrices.')
    end
    
    if(~isstring(input_variable) && iscell(input_variable))
        % Reject cell data
        error('This function can''t handle cell data. Please convert it.')
    end
    
    if(length(varargin) == 1)
        if(~ischar(varargin{1}))
            %class(varargin{1}) % debug.
            error('The optional variable name argument must be a string.')
        end
    end
    
    %% Let's process the variable types we can!
    
    variable_name = inputname(1); % This one fetches the input variable's name.
    if(isempty(variable_name))
        if(length(varargin) == 1)
            variable_name = varargin{1}; % Assign the name from the second string
        else
            error('You gave this function data, and you didn''t specify the variable name.')
        end
    end
    
    output_string = ''; % Initialise this as empty string.
    %...and now, we literally do a bunch of if statements.
    
    %% Boolean
    if(islogical(input_variable))
        % We need to make sure that a boolean is in fact a boolean.
        if(input_variable == true)
            output_string = sprintf('\t<boolean variable_name="%s">True</boolean>\n', variable_name);
        end

        if(input_variable == false)
            output_string = sprintf('\t<boolean variable_name="%s">False</boolean>\n', variable_name);
        end
    end
 
    %% String
    if(ischar(input_variable))
        output_string = sprintf('\t<string variable_name="%s">%s</string>\n', variable_name, input_variable);
    end

    %% Matrix
    if(~islogical(input_variable))
        % This is needed, because otherwise this bit will interpret a
        % boolean as a single number.
        if(ismatrix(input_variable) && ~ischar(input_variable))
            [rows, columns] = size(input_variable); % We need to know the dimensions of the matrix. It can be a 1-by-1 matrix.
            matrix_string_preamble = sprintf('\t<matrix ncols="%d" nrows="%d" variable_name="%s">\n', columns, rows, variable_name);
            matrix_string_pulp = []; % This has to be dynamic, because the row and column numbers might change.
            for(i = 1:rows)
                matrix_row_string = sprintf('\t\t<row_%d>\n', i-1);
                matrix_column_string = [];
                for(j = 1:columns)
                    matrix_column_string = [matrix_column_string, sprintf('\t\t\t<col_%d>%f</col_%d>\n', j-1, input_variable(i, j), j-1)]; % We append to the columns string too.
                end
                matrix_row_string = [matrix_row_string, matrix_column_string, sprintf('\t\t</row_%d>\n', i-1)]; % We assemble a rows's worth of data
                matrix_string_pulp = [matrix_string_pulp, matrix_row_string]; % and shove it to the pulp
            end
            output_string = [matrix_string_preamble, matrix_string_pulp, sprintf('\t</matrix>\n')];

        end
    end
end

