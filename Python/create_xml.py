# This script simply generates an xml file, and includes different variables in it.
from lxml import etree
import numpy as np
import datetime

# measure performance
from timeit import default_timer as timer

start = timer()

# Say, we have a bunch of variables. We work with numpy, because we might do some mambo jambo with the data on the server side.
number_triplet = np.matrix([1.03, 32.76, 10.001])
quaternion = np.matrix([ [0, 0, 0, 0], [1, 2, 4, 5], [12, 12, 43, 43], [8, 8, 8, 8] ])
string = "Woohoo. This was sent from Python."
button_status = True

# Now we add these variables together. This time, manually.
xml_root = etree.Element("dataFrame", created_at = "%s" % (datetime.datetime.now().timestamp()) ) # Create the root of the xml. %This
triplet_shape = np.shape(number_triplet)
child1 = etree.SubElement(xml_root, "matrix", variable_name = "python_triplet", nrows = "%s" % triplet_shape[0], ncols = "%s" % triplet_shape[1], )
# Larger things can be added with recursions. Let's make this arbitrary! Create the statement as a string, and then execute.
for i in range(triplet_shape[0]):
    sub_child_opening_string = "child1_%d = etree.SubElement(child1, \"row_%d\")" % (i, i) # Create he opening string
    exec(sub_child_opening_string)
    for j in range(triplet_shape[1]):
        inside_opening_string = "child1_%d_%d = etree.SubElement(child1_%d, \"col_%d\")" % (i, j, i, j)
        exec(inside_opening_string)
        inside_data_adder_string = "child1_%d_%d.text = np.array2string(number_triplet[i, j])" % (i, j)
        exec(inside_data_adder_string)



# Get the shape
matrix_shape = np.shape(quaternion)
child2 = etree.SubElement(xml_root, "matrix", variable_name = "python_matrix", nrows = "%s" % matrix_shape[0], ncols = "%s" % matrix_shape[1], )
# Larger things can be added with recursions. Let's make this arbitrary! Create the statement as a string, and then execute.
for i in range(matrix_shape[0]):
    sub_child_opening_string = "child2_%d = etree.SubElement(child2, \"row_%d\")" % (i, i) # Create he opening string
    exec(sub_child_opening_string)
    for j in range(matrix_shape[1]):
        inside_opening_string = "child2_%d_%d = etree.SubElement(child2_%d, \"col_%d\")" % (i, j, i, j)
        exec(inside_opening_string)
        inside_data_adder_string = "child2_%d_%d.text = np.array2string(quaternion[i, j])" % (i, j)
        exec(inside_data_adder_string)

# String is a string. Nothing to see here.
child3 = etree.SubElement(xml_root, "string", variable_name = "python_string")
child3.text = string

# Convert a boolean to a string.
child4 = etree.SubElement(xml_root, "boolean", variable_name = "python_boolean")
child4.text = "%s" % button_status


#xml_string = etree.tostring(xml_root, pretty_print = True, xml_declaration = True)
xml_string = etree.tostring(xml_root, pretty_print = True)
end = timer()
print(end-start)

#print(xml_string)
# Write this out to a file
xml_file = open("python_generated.xml", 'w')
# Prepare the file to write, effectively we convert a byte array to a string.
test = xml_string.decode('ASCII')
xml_file.write(test)

xml_file.close()
