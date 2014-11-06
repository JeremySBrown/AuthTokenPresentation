AuthTokenPresentation
=====================

Code sample for my Raleigh Code Camp 2014 Presentation

Before building solution make sure NuGet Package Restore is enabled. (Right-Click Solution Icon)

Running both API Site and Client Site:
1. Make sure APIFinal Project is the default project.
2. Start the project without Debugging (CTRL-F5)
3. Right-Click WebClient01 Project and select View/View In Browser
4. The web client project has text field for the URL of the API project. Make sure they match

Database Init:
The database is generated from Entity Framework Code First, and is generated on the first attempt to connect it. This make take a few seconds to complete.
You can also do so by Refreshing the APIFinal page after running step 2 from above.

Three users are created during the database setup:
Admin
User01
User02

Each user has the same password of 123abc.


