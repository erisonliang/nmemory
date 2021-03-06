---
permalink: cascaded-operation
---

## Introduction

A foreign key with cascade delete means that if a record in the parent table is deleted, then the corresponding records in the child table will automatically be removed and this is called a cascade delete.

A foreign key with cascade delete can be created using **RelationOptions** object with cascad option in **TableCollection.CreateRelation** method.

{% include template-example.html %} 
{% highlight csharp %}

RelationOptions options = new RelationOptions(
                            cascadedDeletion: true);

var foreignIndex = this.members.CreateIndex(
                    new RedBlackTreeIndexFactory(),
                    m => m.GroupId);

this.Tables.CreateRelation(this.groups.PrimaryKeyIndex,
                        foreignIndex,
                        x => x.Value,
                        x => x, 
                        options);
{% endhighlight %}

The following example will delete the member as well when a group of Id equal to 1 is deleted.

{% include template-example.html %} 
{% highlight csharp %}
MyDatabase database = new MyDatabase();

database.Groups.Insert(new Group { Name = "Group 1" });
database.Groups.Insert(new Group { Name = "Group 2" });
database.Members.Insert(new Member { Id = 1, Name = "John Doe", GroupId = 1 });

database.Groups.Delete(new Group { Id = 1 });
{% endhighlight %}


