# Microsoft Dynamics 365 / Power App Tools for Visual Studio

Microsoft Dynamics 365 / Power App Tools gives you an easy way to generate code, update and publish web resources directly from Visual Studio.

### Credits

  + Marat Deykun - [Blog](http://happycrm.blogspot.com/p/crm-publisher.html)
	+ Base code: https://archive.codeplex.com/?p=crmupdater
    + Base Installer & Documentation: [Visual Studio Gallery](https://marketplace.visualstudio.com/items?itemName=MaratVDeykun.MicrosoftDynamicsCRMWebResourcesUpdater)
 

  + Dynamics CRM Code Generator for Visual Studio
    + forked from [xairrick/CrmCodeGenerator](https://github.com/xairrick/CrmCodeGenerator)

 + My Additions (D365-VS-Tools)
    + Add/Update JavaScript File Version on file upload	
    + Update CRM Solution Version on file upload    
    + JSON Based selection of Entities & Attributes with configurable Code Names.
    + Integrarion of [MscrmTools.Xrm.Connection (from XrmToolKit)](https://github.com/MscrmTools/MscrmTools.Xrm.Connection)

#### JavaScript File Version
Add/Update File Version (Date/Time as Comment) into each JavaScript Files each time before upload. (Just if file content change)

Current Version Format: ```// Version: 2019-02-27 09:30```

####	CRM Solution Version
will be updated after upload some files (if none file upload happen, solution version not change). Solution is not published after the Version is changed, just the web resources.
The Solution Version will be updated following the follow rules:

Format: ```[defined by user].[defined by user].yyMMdd.HHmm```

Examples: 
+ User Set:  ```9.1.0.0```
  + Version will be on 27 Feb. 2019, 10:30 set to:  ```9.1.190227.1030``` 
  + Version will updated on 28 Feb. 2019, 15:30 set to:  ```9.1.190228.1530``` 
+ From: ```<empty string>``` => ```1.0.190228.1530```
+ From: ```1``` => ```1.0.190228.1530```
+ From: ```9.1``` => ```9.1.190228.1530```
+ From: ```prod``` => ```prod.0.190228.1530```

#### Limitations
+ Other versions formats are not supported
+ UTC Time is used. (```C# DateTime.UtcNow```)
+ Not configurable

### Changes
-
#### _v0.5.40.1 (2021-06-06)_
+ Updating the Connnection Control from McTools (some connections types are not working - use SDK Dialog)
#### _v0.5.35.8 (2021-03-10)_
+ Add Visual Studio 2019 compatibility
#### _v0.5.35.6 (2019-03-14)_
+ Add/Update JavaScript File Version on file upload	
+ Update CRM Solution Version on file upload
+ Upgrade Package References

