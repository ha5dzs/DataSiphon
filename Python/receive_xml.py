# This script receives an XML script.

# /usr/local/Cellar/python/3.7.2_1/bin/python3
from lxml import etree
import numpy as np
import datetime
# measure performance
from timeit import default_timer as timer


#file_name = "python_generated.xml"
file_name = "csharp_generated.xml"

# We need to load the file in as a string.
xml_file = open(file_name, 'r')
xml_string = xml_file.read()
xml_file.close()

start = timer()

# If this fails, there was something wrong with the xml file.
xml_root = etree.fromstring(xml_string)
# Now we can fetch the variables.
time_at_creation = float(xml_root.get("created_at")) # Fetches UNIX time.

for child in xml_root:
    #print(child.tag) # debug.
    if child.tag == "string":
        # If we got here, then the nth element is a string.
        variable_name = child.get("variable_name") # This fetches the variable name from the specified attribute
        enclosed_text = child.text # This gets the data as text.
        string_to_execute = variable_name + "=" + "\"" + enclosed_text + "\"" # Prepare the string for execution
        #print(string_to_execute) # debug
        exec(string_to_execute) # Load the variable into the memory
    
    if child.tag == "boolean":
        # Booleans are easy.
        variable_name = child.get("variable_name") # Fetch the "variable_name" attribute
        enclosed_text = child.text
        # Now we need to check for the two possible values.
        if enclosed_text == "False":
            string_to_execute = variable_name + "=" + "False"
        elif enclosed_text == "True":
            string_to_execute = variable_name + "=" + "True"
        else:
            # If we got here, there is some other string stored in the boolean, which shouldn't happen. Perhaps throw a warning.
            pass
        #print(string_to_execute) # debug
        exec(string_to_execute) # Load the variable into the memory

    if child.tag == "matrix":
        # Now this is complicated. Let's process the obvious stuff first:
        variable_name = child.get("variable_name") # Get the variable name
        no_of_rows = int(float(child.get("nrows"))) # number of rows, cast as an int
        no_of_columns = int(float(child.get("ncols"))) # number of columns, cast as an int
        
        # Since I saved everything as a matrix, we need to filter special cases
        if (no_of_columns == 1) & (no_of_rows == 1):
            # If we got here, the matrix is just a single number.
            single_number_as_string = child.xpath("string()")
            # We have a bunch of newlines everywhere. I think this is due to the indentation Matlab has generated.
            single_number_as_string = single_number_as_string.rstrip("\t\n")
            single_number_as_string = single_number_as_string.lstrip("\t\n")
            string_to_execute = variable_name + "=" + single_number_as_string
            #print(string_to_execute) # debug.
            exec(string_to_execute)
        else:
            # If we got here, we have to process it as a matrix.
            matrix_as_string = "[" # this string will be executed. We are creating a matrix declaration as a string.
            for i in range(0, no_of_rows):
                if(no_of_rows > 1):
                    matrix_as_string = matrix_as_string + "[ " # Opening bracket for a row, but only when multiple rows are necessary.
                for j in range(0, no_of_columns):
                    # Add to the string in this loop.
                    matrix_element = child.xpath("row_" + str(i) + "/col_" + str(j)) # This one selects the correct element, and returns it as a list
                    element_text = matrix_element[0].text # We get the string out of the list
                    #print(element_text) # debug.
                    # And now we can add the desired number.
                    if(j == (no_of_columns-1)):
                        matrix_as_string = matrix_as_string + element_text + " " # at the end of the row, don't put the comma on
                    else:
                        matrix_as_string = matrix_as_string + element_text + ", " # Put the comma on at interim rows.
                if(i == (no_of_rows-1)):
                    matrix_as_string = matrix_as_string + "]" # At the end of all the rows, don't put a comma on
                else:
                    matrix_as_string = matrix_as_string + "], " # Put a comma on between interim rows.
            if(no_of_rows > 1):
                matrix_as_string = matrix_as_string + "]" # Whack a closing bracket to the end!
            #print(matrix_as_string) # debug
            string_to_execute = variable_name + "=" + "np.matrix(" + matrix_as_string + ")" # Assemble the string that declares the variable and fills its data
            #print(string_to_execute) # debug
            exec(string_to_execute)
end = timer()
print(end-start)