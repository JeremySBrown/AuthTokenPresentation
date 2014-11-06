AuthTokenPresentation
=====================

Code sample for my Raleigh Code Camp 2014 Presentation

<p>
Before building solution make sure NuGet Package Restore is enabled. (Right-Click Solution Icon)
</p>

Running both API Site and Client Site:
<ol>
<li>Make sure APIFinal Project is the default project.</li>
<li>Start the project without Debugging (CTRL-F5)</li>
<li>Right-Click WebClient01 Project and select View/View In Browser</li>
<li>The web client project has text field for the URL of the API project. Make sure they match</li>
</ol>


Database Init:
<p>The database is generated from Entity Framework Code First, and is generated on the first attempt to connect it. This make take a few seconds to complete.</p>
<p>You can also do so by Refreshing the APIFinal page after running step 2 from above.</p>

Three users are created during the database setup:
Admin, User01, User02

Each user has the same password of 123abc.


