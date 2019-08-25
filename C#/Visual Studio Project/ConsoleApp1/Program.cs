using System;
using System.Globalization; // This is needed for the number formatting.
using System.IO; // This is for generating and reading the file. and to interact with streams
using System.Text; // String management
using System.Xml; // This is required for XML parsing. If you use this, compile it with mcs /reference:System.Xml.dll <your_c#_code.cs>
using System.Threading.Tasks; // This is for the asynchronous operation.

// We define this class, and use these 'methods' inside it.
public class DataSiphon
{
    public static string BeginHeader()
    {
        // This one creates the XML file header. It includes the creation time in the unix format, to microsecond precision.
        DateTime Epoch = new DateTime(1970, 1, 1); // Epoch
        DateTime TimeNow = DateTime.UtcNow; // Local time zone.

        long elapsedTicks = TimeNow.Ticks - Epoch.Ticks;
        TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);

        string TimeString = String.Format(CultureInfo.InvariantCulture, "{0:0.000000}", elapsedSpan.TotalSeconds);


        return string.Concat("<dataFrame created_at=\"", TimeString, "\"", ">\n");
    }

    public static string AddChildString(string InputString, string VariableName) {
        /* 
            This method assembles a string child, and outputs the xml string accordingly.
            Input arguments are:
                -> InputString, which is a string you want to include in the xml string.
                -> VariableName, which is also a string, and is going to be the name of the string you want to send to other systems.
        */
        
        string XmlStringString = string.Concat("\t<string variable_name=\"", VariableName, "\">", InputString, "</string>\n");

        return XmlStringString;
    }

    public static string AddChildBoolean(bool InputBool, string VariableName) {
        /*
            This method assembles a boolean child, and outputs the xml string accordingly
            Input arguments are:
                -> InputBool, which is a string you want to include in the xml string.
                -> VariableName, which is also a string, and is going to be the name of the string you want to send to other systems.
         */

        string XmlBoolString = "\t<boolean variable_name=\"";

        if(InputBool == true) {
            XmlBoolString = string.Concat(XmlBoolString, VariableName, "\">", "True", "</boolean>\n");
        } else {
            XmlBoolString = string.Concat(XmlBoolString, VariableName, "\">", "False", "</boolean>\n");
        }

        return XmlBoolString;
    }

    public static string AddChildMatrix( double[,] InputMatrix, string VariableName) {
        /*
            This method assembles a matrix child, and output the xml string accordingly.
            The matrix can be up to two dimensional, and of any size, type double, including just a single value.
            Input arguments are:
                -> InputMatrix, which is a string you want to include in the xml string.
                -> VariableName, which is also a string, and is going to be the name of the string you want to send to other systems.
         */

        // First of all, get the dimensions of the matrix.
        int Rows = InputMatrix.GetLength(0);
        int Columns = InputMatrix.GetLength(1);

        // Now we can assemble the matrix preamble
        string XmlMatrixStringPreamble = string.Concat("\t<matrix ncols=\"", Columns, "\" nrows=\"", Rows, "\" variable_name=\"", VariableName, "\">\n");

        // Iniitialise empty string. This is the pulp.
        string XmlMatrixStringPulp = ""; 
        // ...and now we read the matrix recursively.
        for(int i = 0; i < Rows; i++) {
            string XmlMatrixRowString = string.Format("\t\t<row_{0}>\n", i);
            string XmlMatrixColumnString; // empty as well, for the columns
            for(int j = 0; j < Columns; j++) {


                // This is the statement that extracts the data and formats it
                string XmlMatrixDataString = string.Format(CultureInfo.InvariantCulture, "{0}", InputMatrix[i, j]);

                XmlMatrixColumnString = string.Concat("\t\t\t<col_", j, ">", XmlMatrixDataString, "</col_", j, ">\n" );
                // ...and this one adds it to the row strings.
                XmlMatrixRowString = string.Concat(XmlMatrixRowString, XmlMatrixColumnString);
            }
            // Now that we have a full row assembled, add it to the pulp
            XmlMatrixStringPulp = string.Format("{0}{1}\t\t</row_{2}>\n", XmlMatrixStringPulp, XmlMatrixRowString, i); 
        }
        // ...and finally, we put it all together.
        string XmlMatrixString = string.Concat(XmlMatrixStringPreamble, XmlMatrixStringPulp, "\t</matrix>\n");


        return XmlMatrixString;
    }

    public static string AddChildDouble( double InputDouble, string VariableName) {
        /*
            This method assembles a matrix child, and output the xml string accordingly.
            In C# if only a single double value is passsed without being cast as a matrix, AddChildMatrix() will fail.
            This one won't, but it still saves the data as a 1-by-1 matrix.
            Input arguments are:
                -> InputMatrix, which is a string you want to include in the xml string.
                -> VariableName, which is also a string, and is going to be the name of the string you want to send to other systems.
         */

        // First of all, get the dimensions of the matrix.
        int Rows = 1;
        int Columns = 1;

        // Now we can assemble the matrix preamble
        string XmlMatrixStringPreamble = string.Concat("\t<matrix ncols=\"", Columns, "\" nrows=\"", Rows, "\" variable_name=\"", VariableName, "\">\n");

        // Iniitialise empty string. This is the pulp.
        string XmlMatrixStringPulp = ""; 
        // ...and now we read the matrix recursively.
        for(int i = 0; i < Rows; i++) {
            string XmlMatrixRowString = string.Format("\t\t<row_{0}>\n", i);
            string XmlMatrixColumnString; // empty as well, for the columns
            for(int j = 0; j < Columns; j++) {


                // This is the statement that extracts the data and formats it
                string XmlMatrixDataString = string.Format(CultureInfo.InvariantCulture, "{0}", InputDouble);

                XmlMatrixColumnString = string.Concat("\t\t\t<col_", j, ">", XmlMatrixDataString, "</col_", j, ">\n" );
                // ...and this one adds it to the row strings.
                XmlMatrixRowString = string.Concat(XmlMatrixRowString, XmlMatrixColumnString);
            }
            // Now that we have a full row assembled, add it to the pulp
            XmlMatrixStringPulp = string.Format("{0}{1}\t\t</row_{2}>\n", XmlMatrixStringPulp, XmlMatrixRowString, i); 
        }
        // ...and finally, we put it all together.
        string XmlMatrixString = string.Concat(XmlMatrixStringPreamble, XmlMatrixStringPulp, "\t</matrix>\n");


        return XmlMatrixString;
    }

    public static string EndHeader() {
        // This one finishes off the root of the xml.
        return "</dataFrame>\n";
    }


    /*
            RECEIVE METHODS
     */

    public static (bool, string) FetchString(string XmlString, string VariableName) {
        /* 
            This method looks for a string with a given variable in a string of XML syntax.
            It literally looks for <string variable_name="XXX"> component and reads the text afterwards.
            This means that you need to know the type and the name of the variable to extract.
            Input arguments are:
                -> XmlString, which is a string of ASCII-encoded syntax
                -> VariableName, which is also a string, containing the variable with the given name.
            Returns:
            -fail, which is a boolean. Set to false if the string was found, and set to true when the string was found.
            -The string in the XML file. Returns an empty string if the given variable is not found.
        */
            byte[] XmlByteArray = Encoding.ASCII.GetBytes( XmlString ); // Step 1: From string to byte array.
            MemoryStream XmlStream = new MemoryStream( XmlByteArray); // Step 2: From byte array to stream


            XmlReaderSettings settings = new XmlReaderSettings(); // This is needed for the XmlReader
            //settings.Async = true; // We won't do this asynchronous stuff.

            XmlReader Reader = XmlReader.Create(XmlStream, settings); // Set up XmlReader object
            // Scan through the lot.

            bool VariableTypeMatch = false; // This is set to true when the xml string contains the type of variable we are looking for.
            bool FoundVariableWeWant = false; // This is set to true when we found the variable we are looking for in the input argument.
            bool ReadyToRetun = false; // Final semaphore for the return statement evaluation.
            string StringToReturn = "";
            do {
                switch (Reader.NodeType) {
                    case XmlNodeType.Element:
                        if(Reader.Name.Equals("string") == true) {
                            // If we got here we have a variable type match.
                            //Console.WriteLine("I found a string.\n");
                            VariableTypeMatch = true;
                            // Once we found the variable type we want, scan through the attributes.
                            while (Reader.MoveToNextAttribute()) {
                                if(Reader.Name.Equals("variable_name") && Reader.Value.Equals(VariableName)) {
                                    // If we got here, we found the variable name we want.
                                    FoundVariableWeWant = true;
                                    //Console.WriteLine("Found the variable too!\n");
                                    
                                }
                                //Console.Write("Attribute {0} is set to {1}\n", Reader.Name, Reader.Value);
                            }
                        }
                    break;
                    
                    case XmlNodeType.Text:
                        if(VariableTypeMatch == true && FoundVariableWeWant == true) {
                            // If we got here, we found our variable!
                            //Console.Write("...and the pulp is: {0}\n", Reader.Value);
                            StringToReturn = Reader.Value;
                            ReadyToRetun = true;
                            // ...and to make sure that nothing else gets read out
                            VariableTypeMatch = false;
                            FoundVariableWeWant = false;

                        }
                        
                    break;
                    
                    case XmlNodeType.EndElement:
                        //Console.Write("</{0}>", Reader.Name);
                    break;
                }       
            }  while (Reader.Read());    


            // Send back the contents.
            if(ReadyToRetun == true) {
                // If we found our string, return it.
                return (false, StringToReturn);
            } else {
                // If we got here, the string wasn't found. Indicate fail and push back an empty string
                return (true, StringToReturn); // If nothing touched this one during execution, this will stay an empty string.
            }

    }

     public static (bool, bool) FetchBool(string XmlString, string VariableName) {
        /* 
            This method looks for a boolean with a given variable in a string of XML syntax.
            It literally looks for <bool variable_name="XXX"> component and reads the text afterwards.
            This means that you need to know the type and the name of the variable to extract.
            Input arguments are:
                -> XmlString, which is a string of ASCII-encoded syntax
                -> VariableName, which is also a string, containing the variable with the given name.
            Returns:
            -fail, which is a boolean. Set to false if the string was found, and set to true when the string was found.
            -The boolean value. True or false. Of couse, if fail is true, then the return varable is invalid.
        */
            byte[] XmlByteArray = Encoding.ASCII.GetBytes( XmlString ); // Step 1: From string to byte array.
            MemoryStream XmlStream = new MemoryStream( XmlByteArray); // Step 2: From byte array to stream


            XmlReaderSettings settings = new XmlReaderSettings(); // This is needed for the XmlReader
            //settings.Async = true; // We won't do this asynchronous stuff.

            XmlReader Reader = XmlReader.Create(XmlStream, settings); // Set up XmlReader object
            // Scan through the lot.

            bool VariableTypeMatch = false; // This is set to true when the xml string contains the type of variable we are looking for.
            bool FoundVariableWeWant = false; // This is set to true when we found the variable we are looking for in the input argument.
            bool ReadyToRetun = false; // Final semaphore for the return statement evaluation.
            bool BoolToReturn = false; // Output variable.
            do {
                switch (Reader.NodeType) {
                    case XmlNodeType.Element:
                        if(Reader.Name.Equals("boolean") == true) {
                            // If we got here we have a variable type match.
                            //Console.WriteLine("I found a string.\n");
                            VariableTypeMatch = true;
                            // Once we found the variable type we want, scan through the attributes.
                            while (Reader.MoveToNextAttribute()) {
                                if(Reader.Name.Equals("variable_name") && Reader.Value.Equals(VariableName)) {
                                    // If we got here, we found the variable name we want.
                                    FoundVariableWeWant = true;
                                    //Console.WriteLine("Found the variable too!\n");
                                    
                                }
                                //Console.Write("Attribute {0} is set to {1}\n", Reader.Name, Reader.Value);
                            }
                        }
                    break;
                    
                    case XmlNodeType.Text:
                        if(VariableTypeMatch == true && FoundVariableWeWant == true) {
                            // If we got here, we found our variable!
                            //Console.Write("...and the pulp is: {0}\n", Reader.Value);
                             if(Reader.Value.Equals("True")) {
                                 // We capitalised in the xml string, we need to take this into account when comparing.
                                 BoolToReturn = true;
                                 ReadyToRetun = true;
                             } 
                             if(Reader.Value.Equals("False")) {
                                 // Let's check for false values too.
                                 BoolToReturn = false;
                                 ReadyToRetun = true; 
                             }
                            // If neither of these statements return a useful value, then the inside of the xml tag was corrupted.


                            // ...and to make sure that nothing else gets read out
                            VariableTypeMatch = false;
                            FoundVariableWeWant = false;

                        }
                        
                    break;
                    
                    case XmlNodeType.EndElement:
                        //Console.Write("</{0}>", Reader.Name);
                    break;
                }       
            }  while (Reader.Read());    


            // Send back the contents.
            if(ReadyToRetun == true) {
                // If we found our string, return it.
                return (false, BoolToReturn);
            } else {
                // If we got here, the string wasn't found. Indicate fail and push back an empty string
                return (true, BoolToReturn); // If nothing touched this one during execution, this will stay an empty string.
            }

    }

    public static (bool, double) FetchDouble(string XmlString, string VariableName) {
        /* 
            This function looks for a 1-by-1 matrix in the given XML string.
            Input arguments are:
                -> XmlString, which is a string of ASCII-encoded syntax
                -> VariableName, which is also a string, containing the variable with the given name.
            Returns:
            -fail, which is a boolean. Set to false if the string was found, and set to true when the string was found.
            -The The double-precision single value it extracted from the the xml data.
        */
            byte[] XmlByteArray = Encoding.ASCII.GetBytes( XmlString ); // Step 1: From string to byte array.
            MemoryStream XmlStream = new MemoryStream( XmlByteArray); // Step 2: From byte array to stream


            XmlReaderSettings settings = new XmlReaderSettings(); // This is needed for the XmlReader
            //settings.Async = true; // We won't do this asynchronous stuff.

            XmlReader Reader = XmlReader.Create(XmlStream, settings); // Set up XmlReader object
            // Scan through the lot.
            
            bool VariableTypeMatch = false; // This is set to true when the xml string contains the type of variable we are looking for.
            bool VariableNameMatch = false; // This is set to true when we found the variable we are looking for in the input argument.
            bool VariableRowsMatch = false; // Since Doubles are stored as a 1-by-1 matrix, we need this.
            bool VariableColsMatch = false; // Since Doubles are stored as a 1-by-1 matrix, we need this.
            bool ReadyToRetun = false; // Final semaphore for the return statement evaluation.
            double DoubleToReturn = 0F; // Output variable.
            do {
                switch (Reader.NodeType) {
                    case XmlNodeType.Element:
                        if(Reader.Name.Equals("matrix") == true) {
                            // If we got here we have a variable type match.
                            //Console.WriteLine("I found a string.\n");
                            VariableTypeMatch = true;
                            // Once we found the variable type we want, scan through the attributes.
                            while (Reader.MoveToNextAttribute()) {
                                if(Reader.Name.Equals("variable_name") && Reader.Value.Equals(VariableName)) {
                                    // If we got here, we found the variable name we want.
                                    VariableNameMatch = true;
                                    //Console.WriteLine("Found the variable too!\n");
                                }
                                if(Reader.Name.Equals("ncols") && Reader.Value.Equals("1")) {
                                    // Is this a single-column matrix?
                                    VariableColsMatch = true;
                                    //Console.WriteLine("Found the variable too!\n");
                                }
                                if(Reader.Name.Equals("nrows") && Reader.Value.Equals("1")) {
                                    // Is this a single-row matrix?
                                    VariableRowsMatch = true;
                                    //Console.WriteLine("Found the variable too!\n");
                                }
                                //Console.WriteLine("Matches: Type={0}, Name={1}, RowsMatch={2}, ColsMatch={3}\n", VariableTypeMatch, VariableNameMatch, VariableRowsMatch, VariableColsMatch);
                                //Console.Write("Attribute {0} is set to {1}\n", Reader.Name, Reader.Value);
                            }
                        }
                    break;
                    
                    case XmlNodeType.Text:
                        if(VariableTypeMatch == true && VariableNameMatch == true && VariableRowsMatch == true && VariableColsMatch == true) {
                            // If we got here, we found our variable!
                            //Console.Write("...and the pulp is: {0}\n", Reader.Value);
                 
                            // If neither of these statements return a useful value, then the inside of the xml tag was corrupted.
                            DoubleToReturn = double.Parse(Reader.Value, System.Globalization.CultureInfo.InvariantCulture); // Regional settings can override the decimal point and separation.
                            //Console.WriteLine("DoubleToReturn: {0}\n", DoubleToReturn);
                            ReadyToRetun = true; // We found our value!
                            // ...and to make sure that nothing else gets read out
                            VariableTypeMatch = false; 
                            VariableNameMatch = false;
                            VariableRowsMatch = false;
                            VariableColsMatch = false; 

                        }
                        
                    break;
                    
                    case XmlNodeType.EndElement:
                        //Console.Write("</{0}>", Reader.Name);
                    break;
                }       
            }  while (Reader.Read());    


            // Send back the contents.
            if(ReadyToRetun == true) {
                // If we found our string, return it.
                return (false, DoubleToReturn);
            } else {
                // If we got here, the string wasn't found. Indicate fail and push back an empty string
                return (true, DoubleToReturn); // If nothing touched this one during execution, this will stay an empty string.
            }

    }


    public static (bool, double[,]) FetchMatrix(string XmlString, string VariableName) {
        /* 
            This method is the main course: It fetches a matrix from the given XML string.
            The matrix may only contain numbers, and it an have any number of rows and columns.
            Input arguments are:
                -> XmlString, which is a string of ASCII-encoded syntax
                -> VariableName, which is also a string, containing the variable with the given name.
            Returns:
            -fail, which is a boolean. Set to false if the string was found, and set to true when the string was found.
            -The matrix.
        */
            //byte[] XmlByteArray = Encoding.ASCII.GetBytes( XmlString ); // Step 1: From string to byte array.
            //MemoryStream XmlStream = new MemoryStream( XmlByteArray); // Step 2: From byte array to stream

            // Unlike the other functions which use XmlReader, we use XmlDocoment here. We have sub-nodes to parse too.
            XmlDocument XmlObject = new XmlDocument();
            XmlObject.LoadXml(XmlString); // Load the string.
            // These are used to check declaration properly.
            bool VariableTypeMatch = false;
            bool VariableNameMatch = false;
            bool VariableFound = false;
            // These will be updated, if we found a matrix.
            int ncols = 0;
            int nrows = 0;
            double[,] ReturnMatrix; // This is where the information will be saved to.
            
            // Print.
            //Console.WriteLine(XmlObject.DocumentElement.OuterXml);

            XmlNode RootNode = XmlObject.FirstChild; // Associate to the root node.

            // Scan through the child nodes, and see if we have one called 'matrix'. Kinda like in the docs.
            if(RootNode.HasChildNodes) {
                for(int i = 0; i<RootNode.ChildNodes.Count; i++) {
                    if(RootNode.ChildNodes[i].Name.Equals("matrix") && VariableFound == false) {
                        // Examine all nodes until we found the variable we are looking for.
                        VariableTypeMatch = true; // This means we have the correct type at hand.

                        // Do we have a name match?
                        if(RootNode.ChildNodes[i].Attributes.GetNamedItem("variable_name").Value.Equals(VariableName)) {
                            // If we got here, we found the variable we are looking for.
                            VariableNameMatch = true; // This means we have the correct name at hand.
                            //Console.WriteLine("We found our variable, {0}\n", RootNode.ChildNodes[i].Attributes.GetNamedItem("variable_name").Value);
                        } else {
                            VariableNameMatch = false;
                        }

                        // Now let's look for the matrix dimensions.
                        // ncols.
                        if(RootNode.ChildNodes[i].Attributes.GetNamedItem("ncols").Value != "") {
                            // if we got here, we can update the number of columns
                            ncols = int.Parse(RootNode.ChildNodes[i].Attributes.GetNamedItem("ncols").Value, System.Globalization.CultureInfo.InvariantCulture);
                        }
                        // nrows
                        if(RootNode.ChildNodes[i].Attributes.GetNamedItem("nrows").Value != "") {
                            // if we got here, we can update the number of columns
                            nrows = int.Parse(RootNode.ChildNodes[i].Attributes.GetNamedItem("nrows").Value, System.Globalization.CultureInfo.InvariantCulture);
                        }

                        if(VariableNameMatch == true && VariableTypeMatch == true) {
                            // If we got here, we can read out our matrix from the XML document.
                            //Console.WriteLine("Variable {0} has {1} rows and {2} columns.\n", VariableName, nrows, ncols);
                            ReturnMatrix = new double[nrows, ncols]; // Re-initialise the output matrix, and fill it with data!
                            for(int j = 0; j<nrows; j++) {
                                for(int k = 0; k<ncols; k++) {
                                    // Ugly nested loop. Someone might improves it in the future.
                                    // We can now manually do the addressing, and the text conversion.
                                    ReturnMatrix[j, k] = double.Parse(RootNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].InnerText, System.Globalization.CultureInfo.InvariantCulture);
                                    //Console.WriteLine("Matrix: Row {0}, Col {1}, value is {2}", j, k, ReturnMatrix[j, k]);
                                }
                            }
                            VariableFound = true; // This tells that we now have something to return.
                        }
                    }
                }
            }



            // Okay, return the result.
            if(VariableFound == true) {
                return (true, ReturnMatrix);
            } else {
                return (false, new double[0, 0]); // Toss back and empty matrix.
            }
            
    }

}

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string CSharpStringToSend = "Woohoo. This was sent from c#!"; // Say, I have a string to send.
            bool CShapBoolToSend = false; // This a boolean value to send
            double[,] CSharpMatrixToSend = { {-1F, 2.11F, 3F}, {1500F, 4000F, 16984F}, {0F, 0F, 1F} }; // Floating-point stuff is double precision.
            double CSharpFloatToSend = 12.343F;

            // Assemble the XML string
            string XmlString = string.Concat(
                DataSiphon.BeginHeader(),
                DataSiphon.AddChildString(CSharpStringToSend, "c_sharp_string"),
                DataSiphon.AddChildBoolean(CShapBoolToSend, "c_sharp_boolean"),
                DataSiphon.AddChildMatrix(CSharpMatrixToSend, "c_sharp_matrix"),
                DataSiphon.AddChildDouble(CSharpFloatToSend, "c_sharp_float"),
                DataSiphon.EndHeader()
            );

            // Write it to a file.
            File.WriteAllText(@"../../csharp_generated.xml", XmlString);

            /*
            // Print the xml string
            Console.Write(DataSiphon.BeginHeader());
            Console.Write(DataSiphon.AddChildString(CSharpStringToSend, "c_sharp_string")); // Add a string.
            Console.Write(DataSiphon.AddChildBoolean(CShapBoolToSend, "c_sharp_boolean")); // Add a boolean.
            Console.Write(DataSiphon.AddChildMatrix(CSharpMatrixToSend, "c_sharp_matrix")); // add a matrix.
            Console.Write(DataSiphon.EndHeader());
            */

            // Now read a different file back as a string, just for the hell of it.
            /*
            // this one gets a string out.
            (bool Fail, string ReturnString) = DataSiphon.FetchString(XmlString, "c_sharp_string");

            if(Fail == true) {
                Console.WriteLine("The string was not found.");
            } else{
                Console.WriteLine("Returned string is {0}\n", ReturnString);
            }
            */
            /*
            // This one gets a boolean out.
            (bool Fail, bool ReturnBool) = DataSiphon.FetchBool(XmlString, "c_sharp_boolean");
            if(Fail == true) {
                Console.WriteLine("The boolean wasn't found.\n");
            } else {
                Console.WriteLine("The boolean's value is {0}\n", ReturnBool);
            }
            */

            /*
            // This one fetches a double from the xml string.
            (bool Fail, double ReturnDouble) = DataSiphon.FetchDouble(XmlString, "c_sharp_float");
            if(Fail == true) {
                Console.WriteLine("Couldn't find a Double.\n");
            } else {
                Console.WriteLine("Double value is: {0}\n", ReturnDouble);
            }
            */

            (bool Fail, double[,] FetchedMatrix) = DataSiphon.FetchMatrix(XmlString, "c_sharp_matrix");
            if(Fail == true) {
                Console.WriteLine(FetchedMatrix); // Print off the fetched matrix.
            }


        }
    }
}
