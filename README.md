# CHT

CHT - Compact heterogeneous tree - A compact human readable text format for representing heterogeneous trees

Most mainstream formats are designed to represent homogenous data. Trying to represent a tree with heterougeneous nodes (for example an abstract syntax tree) using JSON or even NestedText is extremely verbose. In JSON and similar, the emphasis is on the description of each field of an object rather than describing the object as a whole. Modeling a heterogeneous structure requires an auxiliary field which does not actually hold data, but rather serves as a discriminator. In CHT, the focus is instead on the type of each node/object. On the other hand the fields are not mentioned at all - instead the encoded values are mapped based on their order and the type of the node.

## What this is for

The main motivation / primary usecase is data serialization of data for data driven unit testing and pretty printing while debugging different trees that comeup when implementing a language. However the format is rather abstract and I imagine it could be used anywhere JSON is used.

## Example

Take this piece of a source code:

```python
x = [0, 58, 15]
x.append(7)
y = x[3]
print(y)
text = "Some text with spaces"
```

Its AST may look something like this in JSON:

```json
{
  "Kind": "Block",
  "Children": [
    {
      "Kind": "Assignment",
      "Variable": { "Symbol": "x" },
      "Value": {
        "Kind": "List",
        "Children": [
          { "Kind": "Number", "Value": 0 },
          { "Kind": "Number", "Value": 58 },
          { "Kind": "Number", "Value": 15 }
        ]
      }
    },
    {
      "Kind": "MethodCall",
      "Object": { "Symbol": "x" },
      "Method": { "Symbol": "append" },
      "Arguments": [{ "Kind": "Number", "Value": 7 }]
    },
    {
      "Kind": "Assignment",
      "Variable": { "Symbol": "y" },
      "Value": {
        "Kind": "Indexing",
        "Collection": { "Symbol": "x" },
        "Index": { "Kind": "Number", "Value": 3 }
      }
    },
    {
      "Kind": "FunctionCall",
      "Function": { "Symbol": "print" },
      "Arguments": [{ "Symbol": "y" }]
    },
    {
      "Kind": "Assignment",
      "Variable": { "Symbol": "text" },
      "Value": { "Kind": "Text", "Value": "Some text with spaces" }
    }
  ]
}
```

The same AST in CHT:

```perl
Block:
    Assignment: $x List(0 58 15)
    MethodCall: $x $append 7
    Assignment: $y Indexing($x 3)
    FunctionCall: $print $y
    Assignment: $text "Some text with spaces"
```

## Specification

### Semantics

CHT is an abstract format. An interpretation of a CHT file is up to the communicating parties to choose. What this means, is that there are no data types like arrays, integers or strings in CHT. Instead CHT only describes a tree structure of data.
Each node has a value and a list of children.

### Syntax

CHT is whitespace sensitive. The entire document can be a single line, using parentheses to denote nesting, but when human readibility is a concern, nesting can instead be encoded using indentation.

#### Indentation

CHT is an indentation sensitive format. The indentation of each line is the sequence of white space characters up to the first nonwhitespace. The indentation of each line must either stay the same (be exactly the same as the previous line), increase (the previous line is a prefix of the current line), or decrease (be a prefix of a previous line which has occured before). The first line must start at a zero indentation.

#### Special characters

Whitespace characters including a space ` `, tab, newline and others, a number sign `#`, a colon `:`, a quotation mark `"` and parentheses `()` have special meanings in CHT. Other characters can be used freely.

#### Comments

The rest of the line following `# ` (any whitespace) is ignored.

#### Node

Concatenation of the following:

1. One or both of:
1. 1. Raw value: a pieces of text that does not contain a special character. Colons `:` can be present as long as they have digits on both sides.
1. 2. Quoted value: corresponds exactly with a JSON string syntax. That is the node's value is delimited using a pair of `"` and may contain escape sequences using `\`.
1. Zero or more pairs of parens containing zero or more space separated children.
1. Optionally a `:`followed by one of:
1. 1. Inline children: one or more whitespace separated children.
1. 2. Indented children: a newline, and indentation increase, one or more newline separated children an indentation decrease and a newline

An example of purely raw terminal: `$print`, a purely quoted terminal: `"Some text"`, combined terminal: `re"[A-Z]\w*"`, and raw valued node with children: `List(1 2 3)`.

Note that being terminal (`List`) and having 0 children (`List()`) is two different things in CHT (but may mean the same depending on the application).

The following examples all encode the very same tree:

```perl
Parent("1st child" "2nd child" "3rd child")
```

```perl
Parent: "1st child" "2nd child" "3rd child"
```

```perl
Parent:
    "1st child"
    "2nd child"
    "3rd child"
```

```perl
Parent("1st child"): "2nd child" "3rd child"
```

```perl
Parent("1st child"):
    "2nd child"
    "3rd child"
```

## Implementations

CHT has libraries in the following languages:

- [C#](cs)

## Tooling

We have a VS Code extension.
