# Indexes: Analyzers
---

{NOTE: }

* RavenDB uses indexes to facilitate fast queries powered by [**Lucene**](http://lucene.apache.org/), the full-text search engine.  

* The indexing of a single document starts from creating Lucene's **Document** according to an index definition. 
  Lucene processes it by breaking it into fields and splitting all the text from each field into *tokens* (or *terms*) 
  in a process called *tokenization*. Those tokens will be stored in the index, and later will be searched upon.  
  The tokenization process uses an object called an **Analyzer**.  

* The indexing process and its results can be controlled by various field options and by the Analyzers.  

* In this page:  
  * [Understanding Analyzers](../indexes/using-analyzers#understanding-analyzers)  
  * [RavenDB's Default Analyzers](../indexes/using-analyzers#ravendb)  
  * [Full-Text Search](../indexes/using-analyzers#full-text-search)  
  * [Selecting an Analyzer for a Field](../indexes/using-analyzers#selecting-an-analyzer-for-a-field)  
  * [Creating Custom Analyzers](../indexes/using-analyzers#creating-custom-analyzers)  
  * [Manipulating Field Indexing Behavior](../indexes/using-analyzers#manipulating-field-indexing-behavior)  
  * [Ordering When a Field is Searchable](../indexes/using-analyzers#ordering-when-a-field-is-searchable)  

{NOTE/}

---

{PANEL: Understanding Analyzers}

Lucene offers several Analyzers out of the box, and new ones can be [created](../indexes/using-analyzers#creating-custom-analyzers).  

Various Analyzers differ in the way they split the text stream ("tokenize"), 
and in the way they process those tokens in post-tokenization.  

The examples below will use this sample text:  

`The quick brown fox jumped over the lazy dogs, Bob@hotmail.com 123432.`  

### Analyzers that remove common "Stop Words":

{NOTE: }
[Stop words](https://en.wikipedia.org/wiki/Stop_word) (e.g. the, it, a, is, this, who, that...) are often removed to 
narrow search results by including only words that are used less frequently.

If you want to include words such as IT (Information Technology), be aware that these analyzers will recognize IT as 
one of the stop words and remove it from searches. 
This can affect other acronyms such as WHO (World Health Organization) or names such as "The Who" or "The IT Crowd".  

To prevent excluding such stop words, you can either spell out the entire title instead of abbreviating it 
or use an [analyzer that doesn't remove stop words](../indexes/using-analyzers#analyzers-that-do-not-remove-common-stop-words).
{NOTE/}

* **StandardAnalyzer**, which is Lucene's default, will produce the following tokens:  

    `[quick]   [brown]   [fox]   [jumped]   [over]   [lazy]   [dog]   [bob@hotmail.com]   [123432]`  

    Removes common "stop" words  
    Separates with white spaces and punctuation that is followed by white space  
    Converts to lower-case letters so that searches aren't case sensitive  
    Email addresses are one token - a dot that is not followed by a whitespace is considered part of the token.  
    Numbers with hyphen/dash are not separated at the hyphen.  


* **StopAnalyzer** will work similarly, but will not perform light stemming and will only tokenize on white space:  

    `[quick]   [brown]   [fox]   [jumped]   [over]   [lazy]   [dogs]   [bob]   [hotmail]   [com]`  

    Removes numbers and symbols, then separates tokens with these. 
    This means that email and web addresses are separated.  
    Removes common "stop" words  
    Separates with white spaces  
    Converts to lower-case letters so that searches aren't case sensitive  

---

### Analyzers that do not remove common "Stop Words"

* **SimpleAnalyzer** will tokenize on all non-alpha characters and will make all the tokens lowercase:  

    `[the]   [quick]   [brown]   [fox]   [jumped]   [over]   [the]   [lazy]   [dogs]   [bob]   [hotmail]   [com]`  

    Includes common stop words  
    Removes numbers and symbols, then separates tokens with them. 
    This means that email and web addresses are separated.  
    Separates with white spaces  
    Converts to lower-case letters so that searches aren't case sensitive  

* **WhitespaceAnalyzer** will just tokenize on white spaces:  

    `[The]   [quick]   [brown]   [fox]   [jumped]   [over]   [the]   [lazy]   [dogs,]   [Bob@hotmail.com]   [123432.]`  

    Only separates with whitespaces  
    This analyzer preserves upper/lower cases in text, which means that searches will be case-sensitive.  
    Email and web addresses, phone numbers, and other such forms of ID are kept whole

* **KeywordAnalyzer** will perform no tokenization, and will consider the whole text a stream as one token:  

    `[The quick brown fox jumped over the lazy dogs, bob@hotmail.com 123432.]`  

    This analyzer preserves upper/lower cases in text for case-sensitive searches.  
    Useful in situations like IDs and codes where you do not want to separate into multiple tokens.  

---

### Analyzers that tokenize according to the defined number of characters

* **NGramAnalyzer** will tokenize on predefined token lengths, 2-6 chars long, which are defined by `Indexing.Analyzers.NGram.MinGram` and `Indexing.Analyzers.NGram.MaxGram` configuration options:  
  
   `[.c]  [.co]  [.com]  [12]  [123]  [1234]  [12343]  [123432]  [23]  [234]  [2343]  [23432]  [32]  [34]  [343]  [3432]  [43]  [432]  [@h]  [@ho]  [@hot]  [@hotm]  [@hotma]  [ai]  [ail]  [ail.]  [ail.c]  [ail.co]  [az]  [azy]  [b@]  [b@h]  [b@ho]  [b@hot]  [b@hotm]  [bo]  [bob]  [bob@]  [bob@h]  [bob@ho]  [br]  [bro]  [brow]  [brown]  [ck]  [co]  [com]  [do]  [dog]  [dogs]  [ed]  [er]  [fo]  [fox]  [gs]  [ho]  [hot]  [hotm]  [hotma]  [hotmai]  [ic]  [ick]  [il]  [il.]  [il.c]  [il.co]  [il.com]  [ju]  [jum]  [jump]  [jumpe]  [jumped]  [l.]  [l.c]  [l.co]  [l.com]  [la]  [laz]  [lazy]  [ma]  [mai]  [mail]  [mail.]  [mail.c]  [mp]  [mpe]  [mped]  [ob]  [ob@]  [ob@h]  [ob@ho]  [ob@hot]  [og]  [ogs]  [om]  [ot]  [otm]  [otma]  [otmai]  [otmail]  [ov]  [ove]  [over]  [ow]  [own]  [ox]  [pe]  [ped]  [qu]  [qui]  [quic]  [quick]  [ro]  [row]  [rown]  [tm]  [tma]  [tmai]  [tmail]  [tmail.]  [ui]  [uic]  [uick]  [um]  [ump]  [umpe]  [umped]  [ve]  [ver]  [wn]  [zy]`  

   You can override NGram analyzer default token lengths by configuring `Indexing.Analyzers.NGram.MinGram` and `Indexing.Analyzers.NGram.MaxGram` per index e.g. setting them to 3 and 4 accordingly will generate:  

   `[.co]  [.com]  [123]  [1234]  [234]  [2343]  [343]  [3432]  [432]  [@ho]  [@hot]  [ail]  [ail.]  [azy]  [b@h]  [b@ho]  [bob]  [bob@]  [bro]  [brow]  [com]  [dog]  [dogs]  [fox]  [hot]  [hotm]  [ick]  [il.]  [il.c]  [jum]  [jump]  [l.c]  [l.co]  [laz]  [lazy]  [mai]  [mail]  [mpe]  [mped]  [ob@]  [ob@h]  [ogs]  [otm]  [otma]  [ove]  [over]  [own]  [ped]  [qui]  [quic]  [row]  [rown]  [tma]  [tmai]  [uic]  [uick]  [ump]  [umpe]  [ver]  `  

{PANEL/}

{PANEL: RavenDB's Default Analyzers}

RavenDB has three default analyzers that it uses to index text when no other analyzer was specified:  

* **Default Analyzer** - `LowerCaseKeywordAnalyzer`  
* **Default Exact Analyzer** - `KeywordAnalyzer`  
* **Default Search Analyzer** - `RavenStandardAnalyzer`  

You can choose other analyzers to serve as your default analyzers by modifying the [indexing configuration](../server/configuration/indexing-configuration#indexing.analyzers.default).  

**Default Analyzer**

For regular text fields, RavenDB uses a custom analyzer called `LowerCaseKeywordAnalyzer`. Its implementation 
behaves like Lucene's `KeywordAnalyzer`, but it also performs case normalization by converting all characters 
to lower case. That is - RavenDB stores the entire text field as a single token, in a lowercased form. Given 
the same sample text above, `LowerCaseKeywordAnalyzer` will produce a single token:  

`[the quick brown fox jumped over the lazy dogs, bob@hotmail.com 123432.]`  

**Default Exact Analyzer**

For 'exact case' text fields, RavenDB uses Lucene's `KeywordAnalyzer`, which treats the entire text field as one 
token and does not change the case of the original text. To make an index store text with the exact case, see the 
section on changing field indexing behavior [below](../indexes/using-analyzers#manipulating-field-indexing-behavior).  

**Default Search Analyzer**

For full-text search text fields, RavenDB uses `RavenStandardAnalyzer`, which is just an optimized version of 
Lucene's `StandardAnalyzer`. To make an index that allows full-text search, see the section on changing field 
indexing behavior [below](../indexes/using-analyzers#manipulating-field-indexing-behavior).  

{PANEL/}

{PANEL: Full-Text Search}

To allow full-text search on the text fields, you can use the analyzers provided out of the box with Lucene. These are available as part of the Lucene library which ships with RavenDB.  

For most cases, Lucene's `StandardAnalyzer` would be your analyzer of choice. As shown above, this analyzer is aware of e-mail and network addresses when tokenizing. It normalizes cases, filters out common English words, and does some basic English stemming as well.  

For languages other than English, or if you need a custom analysis process, you can roll your own `Analyzer`. It is quite simple and may already be available as a contrib package for Lucene. 
There are also `Collation analyzers` available (you can read more about them [here](../indexes/sorting-and-collation#collation)).  

{PANEL/}

{PANEL: Selecting an Analyzer for a Field}

To index a document field using a specific analyzer, all you need to do is to match it with the field's 
name:  

{CODE-TABS}
{CODE-TAB:csharp:AbstractIndexCreationTask analyzers_1@Indexes\Analyzers.cs /}
{CODE-TAB:csharp:Operation analyzers_2@Indexes\Analyzers.cs /}
{CODE-TABS/}

{INFO: Analyzer Availability}
The analyzer you are referencing must be available to the RavenDB server instance. See the different 
methods of [creating custom analyzers](../indexes/using-analyzers#creating-custom-analyzers).  
{INFO/}

{PANEL/}

{PANEL: Creating Custom Analyzers}

You can write your own custom analyzers as a `.cs` file. Custom analyzers can be defined as:

* **Database Custom Analyzers** - can only be used by the indexes of the database where they are defined.
* **Server-Wide Custom Analyzers** - can be used by indexes on all databases on all servers in the cluster.

A database analyzer can have the same name as a server-wide analyzer. In this situation, the indexes of that 
database will use the database version of the analyzer. So you can think of database analyzers as overriding 
the server-wide analyzers with the same names.  

There are a few ways to create a custom analyzer and add it to your server:  
1. [Using the Studio](../studio/database/settings/custom-analyzers)  
2. Using the Client API  
3. Adding it [directly to RavenDB's binaries](../indexes/using-analyzers#adding-an-analyzer-to-the-binaries)  

### Using the Client API

First, create a class that inherits from abstract `Lucene.Net.Analysis.Analyzer` (you need to reference 
`Lucene.Net.dll`, which is supplied with RavenDB Server package). For example:  

{CODE analyzers_6@Indexes\Analyzers.cs /}

Next, define the analyzer for a specific database using the operation `PutAnalyzersOperation`. Or, to make it 
a server-wide analyzer, use `PutServerWideOperation`. These operations are very similar in how they work. 
Both of them take one parameter: either an `AnalyzerDefinition`, or an array of `AnalyzerDefinition`'s.  

{CODE-BLOCK: csharp}
public class PutAnalyzersOperation
{
    private readonly AnalyzerDefinition[] _analyzersToAdd;
}

public class PutServerWideAnalyzersOperation
{
    private readonly AnalyzerDefinition[] _analyzersToAdd;
}
{CODE-BLOCK/}

By default, the `PutAnalyzersOperation` will apply to the [default database](../client-api/setting-up-default-database) 
of the document store you're using. To target a different database, use the `ForDatabase()` method - read more 
[here](../client-api/operations/how-to/switch-operations-to-a-different-database).  

The `AnalyzerDefinition` object has two properties, `Name` and `Code`:  

{CODE-BLOCK: csharp}
public class AnalyzerDefinition
{
    public string Name { get; set; }
    public string Code { get; set; }
}
{CODE-BLOCK/}

| Parameter | Type | Description |
| - | - | - |
| **Name** | `string` | The class name of your custom analyzer as it appears in your code |
| **Code** | `string` | Compilable csharp code: a class that inherits from `Lucene.Net.Analysis.Analyzer`, the containing namespace, and the necessary `using` statements. |

#### Client API Example

Now let's see how everything fits together.  

{CODE analyzers_7@Indexes\Analyzers.cs /}

### Adding an Analyzer to the Binaries

Another way of adding custom analyzers to RavenDB is to place them next to RavenDB's binaries. Note that it needs to be 
compatible with .NET Core 2.0 (e.g. .NET Standard 2.0 assembly). The fully qualified name needs to be specified for an 
indexing field that is going to be tokenized by the analyzer. This is the only way to add custom analyzers in RavenDB 
versions older than 5.2.  

{PANEL/}

{PANEL: Manipulating Field Indexing Behavior}

By default, each indexed field is analyzed using the '`LowerCaseKeywordAnalyzer`' which indexes a field as a single, lowercased term.  

This behavior can be changed by setting the `FieldIndexing` option for a particular field. The possible values are:  

* `FieldIndexing.Exact`
* `FieldIndexing.Search`
* `FieldIndexing.No`

Setting the `FieldIndexing` option for this field to `Exact` turns off the field analysis. This causes all the 
properties to be treated as a single token and the matches must be exact (case sensitive), using 
the `KeywordAnalyzer` behind the scenes.  

{CODE analyzers_3@Indexes\Analyzers.cs /}

`FieldIndexing.Search` allows performing full-text search operations against the field using the 'StandardAnalyzer' 
by default:  

{CODE analyzers_4@Indexes\Analyzers.cs /}

If you want to disable indexing on a particular field, use the `FieldIndexing.No` option. This can be useful when you want to [store field data in the index](../indexes/storing-data-in-index), but don't want to make it available for querying. However, it will still be available 
for extraction by projections:  

{CODE analyzers_5@Indexes\Analyzers.cs /}

{PANEL/}

{PANEL: Ordering When a Field is Searchable}

When a field is marked as `Search`, sorting must be done using an additional field. More [here](../indexes/querying/sorting#ordering-when-a-field-is-searchable).  

{PANEL/}

## Related Articles

### Indexes

- [Boosting](../indexes/boosting)
- [Storing Data in Index](../indexes/storing-data-in-index)
- [Dynamic Fields](../indexes/using-dynamic-fields)

### Studio
- [Custom Analyzers](../studio/database/settings/custom-analyzers)  
- [Create Map Index](../studio/database/indexes/create-map-index)  
