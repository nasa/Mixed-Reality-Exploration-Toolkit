http://xfront.com/SchemaVersioning.html
---------------------------------------

Schema Design Recommendations:
- Use the same namespace for all Schema versions.
- Give each new Schema version a different filename or a different URL location or both.
- Don't use anonymous types. Instead, use named types.
- If you change a type when you create a new version of a Schema then give the type a different name.
- Change the name of an element's type only if its immediate content has changed.
- Use a version attribute on the root element. If an instance document is a compound document - that is, an assembly of XML fragments - then place a version attribute on the root of each fragment.

Instance Document Design Recommendations:
- Use the schemaLocation attribute to identify the target Schema (i.e., don't have the Schema validator use out-of-band information to identify the target Schema)

Application Design Recommendations:
- Applications should use the tag names to locate data in instance documents. (Applications should be designed to anticipate that the order of tags may change)
- Define a system-wide protocol (e.g., fault reporting mechanism) to be used when an application is unable to process an instance document it receives from another application.
