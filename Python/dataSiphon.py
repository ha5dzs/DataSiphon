# This class contains all the functions needed for XML communication, including parsing and creating XML strings.
from lxml import etree
import numpy as np
import datetime



def createXmlString(variablesAsDict):
    # This method assembles an XML string from the variables packed together as a dict.
    if(type(variablesAsDict) != type(dict())):
        raise ValueError # Crash.

    # Create header.
    xmlRoot = etree.Element("dataFrame", created_at = "%s" % (datetime.datetime.now().timestamp()) ) # Create the root of the xml.
    # Scan through the dict, find the names of the variables and add them to the xml object paying attention to their type.
    # The node names are programmatically generated, to avoid the overwriting of nodes.
    for variableName in variablesAsDict:
        #print(variableName)
        #print("The detected variable types are:\n")
        #print(type(variablesAsDict[variableName]))
        #print("\n")

        # String. This is simple.
        if(type(variablesAsDict[variableName]) == type(str())):
            stringToExecute = 'stringNode_%s = etree.SubElement(xmlRoot, "string", variable_name = variableName)' % (variableName)
            exec(stringToExecute) # This creates a string node
            stringToExecute = 'stringNode_%s.text = variablesAsDict[variableName]' % (variableName)
            exec(stringToExecute) # This copies the string.
        
        # Boolean. This is simple too.
        if(type(variablesAsDict[variableName]) == type(bool())):
            stringToExecute = 'boolNode_%s = etree.SubElement(xmlRoot, "boolean", variable_name = variableName)' % (variableName)
            exec(stringToExecute)
            stringToExecute = 'boolNode_%s.text = "%s"' % (variableName, variablesAsDict[variableName])
            exec(stringToExecute)

        # Numpy matrix.
        if(isinstance(variablesAsDict[variableName], np.matrix)):
            matrixShape = np.shape(variablesAsDict[variableName])
            stringToExecute = 'matrixNode_%s = etree.SubElement(xmlRoot, "matrix", variable_name = "%s", nrows = "%s", ncols = "%s")' % (variableName, variableName, matrixShape[0], matrixShape[1])
            exec(stringToExecute)
            # Larger things can be added with recursions. Let's make this arbitrary! Create the statement as a string, and then execute.
            for i in range(matrixShape[0]):
                subChildOpeningString = "matrixNode_%s_child_%d = etree.SubElement(matrixNode_%s, \"row_%d\")" % (variableName, i, variableName, i) # Create he opening string
                exec(subChildOpeningString)
                for j in range(matrixShape[1]):
                    insideOpeningString = "matrixNode_%s_child_%d_child_%d = etree.SubElement(matrixNode_%s_child_%d, \"col_%d\")" % (variableName, i, j, variableName, i, j)
                    exec(insideOpeningString)
                    insideDataAdderString = "matrixNode_%s_child_%d_child_%d.text = np.array2string(variablesAsDict[\"%s\"][i, j])" % (variableName, i, j, variableName)
                    exec(insideDataAdderString)

        # Float. This is being saved as a 1-by-1 matrix. Literally like the matrix, but with single row ad single column. Yep, I'm lazy :)
        if(isinstance(variablesAsDict[variableName], float)):
            matrixShape = [1, 1]
            stringToExecute = 'matrixNode_%s = etree.SubElement(xmlRoot, "matrix", variable_name = "%s", nrows = "%s", ncols = "%s")' % (variableName, variableName, matrixShape[0], matrixShape[1])
            exec(stringToExecute)
            # Since we only read a float here, these loops will only execute once.
            for i in range(matrixShape[0]):
                subChildOpeningString = "matrixNode_%s_child_%d = etree.SubElement(matrixNode_%s, \"row_%d\")" % (variableName, i, variableName, i) # Create he opening string
                exec(subChildOpeningString)
                for j in range(matrixShape[1]):
                    insideOpeningString = "matrixNode_%s_child_%d_child_%d = etree.SubElement(matrixNode_%s_child_%d, \"col_%d\")" % (variableName, i, j, variableName, i, j)
                    exec(insideOpeningString)
                    insideDataAdderString = "matrixNode_%s_child_%d_child_%d.text = \"%.6f\"" % (variableName, i, j, variablesAsDict[variableName]) # Since this is a float, we don't need addressing
                    exec(insideDataAdderString)



    # Finish the XML object, and return the string.
    return(etree.tostring(xmlRoot, pretty_print = True))



def parseXmlString(xmlString):
# This method parses an XML string, and returns the execution strings as a list. Then in the main code, you need to run each statement in the dict to get the variables.
    returnList = [] # This will be the return variable.
    xmlRoot = etree.fromstring(xmlString)
    timeAtCreation = float(xmlRoot.get("created_at")) # We don't do anything with this at the moment.
    # ...and now we go though the XML, child by child.
    for child in xmlRoot:
        if child.tag == "string":
            # If we got here, then the nth element is a string.
            variableName = child.get("variable_name") # This fetches the variable name from the specified attribute
            enclosedText = child.text # This gets the data as text.

            # Now we can save the variable declaration in a list as a string.
            returnList.append( variableName + "=" + "\"" + enclosedText + "\"") # Prepare the string for execution
            
        if child.tag == "boolean":
            # Booleans are easy.
            variableName = child.get("variable_name") # Fetch the "variable_name" attribute
            enclosedText = child.text
            # Now we need to check for the two possible values.
            if enclosedText == "False":
                returnList.append(variableName + "=" + "False")
            elif enclosedText == "True":
                returnList.append(variableName + "=" + "True")
            else:
                # If we got here, there is some other string stored in the boolean, which shouldn't happen. Perhaps throw a warning.
                pass


        if child.tag == "matrix":
            # Now this is complicated. Let's process the obvious stuff first:
            variableName = child.get("variable_name") # Get the variable name
            nrows = int(float(child.get("nrows"))) # number of rows, cast as an int
            ncols = int(float(child.get("ncols"))) # number of columns, cast as an int
            
            # Since I saved everything as a matrix, we need to filter special cases
            if (ncols == 1) & (nrows == 1):
                # If we got here, the matrix is just a single number.
                singleNumberAsString = child.xpath("string()")
                # We have a bunch of newlines everywhere. I think this is due to the indentation Matlab has generated. These don't fail if it can't strip the new lines, so we are good.
                singleNumberAsString = singleNumberAsString.rstrip("\t\n")
                singleNumberAsString = singleNumberAsString.lstrip("\t\n")
                returnList.append(variableName + "=" + singleNumberAsString)
                
            else:
                # If we got here, we have to process it as a matrix.
                matrixAsString = "[" # this string will be executed. We are creating a matrix declaration as a string.
                for i in range(0, nrows):
                    if(nrows > 1):
                        matrixAsString = matrixAsString + "[ " # Opening bracket for a row, but only when multiple rows are necessary.
                    for j in range(0, ncols):
                        # Add to the string in this loop.
                        matrixElement = child.xpath("row_" + str(i) + "/col_" + str(j)) # This one selects the correct element, and returns it as a list
                        elementText = matrixElement[0].text # We get the string out of the list
                        #print(element_text) # debug.
                        # And now we can add the desired number.
                        if(j == (ncols-1)):
                            matrixAsString = matrixAsString + elementText + " " # at the end of the row, don't put the comma on
                        else:
                            matrixAsString = matrixAsString + elementText + ", " # Put the comma on at interim rows.
                    if(i == (nrows-1)):
                        matrixAsString = matrixAsString + "]" # At the end of all the rows, don't put a comma on
                    else:
                        matrixAsString = matrixAsString + "], " # Put a comma on between interim rows.
                if(nrows > 1):
                    matrixAsString = matrixAsString + "]" # Whack a closing bracket to the end!
                #print(matrix_as_string) # debug
                returnList.append(variableName + "=" + "np.matrix(" + matrixAsString + ")") # Assemble the string that 

    return returnList