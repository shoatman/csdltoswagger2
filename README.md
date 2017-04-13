# Microsoft Graph CSDL to Swagger 2.0

## Intro

This very rough piece of code expands on the work of the oData labs folks.  The original code upon which this is based can be found [here](https://github.com/OData/lab/tree/master/odata2swagger)

## Usage steps

1. Grab the metadata for Microsoft Graph: graph.microsoft.com/v1.0/$metadata and save locally
2. Update the variables inside the console app (Yes I'm very lazy) to:
* Point to the metadata
* Update the output location and file name
3. Run the tool
4. You hopefully have Swagger 2.0 for Microsoft Graph

## Notes and cautions

The Swagger output of this tool has in no way been fully vetted test or otherwise verified.  I strongly suspect you will find issues.  If you do feel free to file them and or fix them.  Also note that 
the CSDL isn't verbose in terms of documentation and therefore the resulting swagger is not particularly wordy.  In addition some annotations that I expected to be present were unavailable... this means that 
in some cases a parameter is described which may not actually be applicable to the operation/path that you're attempting to use.





