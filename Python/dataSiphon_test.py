import dataSiphon # This is a separate module.
import numpy as np

myData = {
    "pythonBoolean": True,
    "pythonString": "This is the string in the dict.",
    "pythonMatrix": np.matrix([ [1, 2, 4, 5], [12, 12, 43, 43], [8, 8, 8, 8] ]),
    "pythonDouble": 325.12445
}

# Create the XML string.
xml_string = dataSiphon.createXmlString(myData).decode("ASCII")
#print(xml_string)

# Parse an XML string.
#file_name = "python_generated.xml"
file_name = "csharp_generated.xml"

# We need to load the file in as a string.
xml_file = open(file_name, 'r')
xml_string = xml_file.read()
#print(xml_string)
xml_file.close()


variables_as_list = dataSiphon.parseXmlString(xml_string)

for item in variables_as_list:
    exec(item)
    print(item + "\n")
pass