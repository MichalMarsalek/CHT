# CHT
CHT - Compact heterogeneous tree - A compact human readable text format for representing heterogeneous trees

Most mainstream formats are designed to represent homogenous data. Trying to represent a tree with heterougeneous nodes (for example an abstract syntax tree) using JSON or even NestedText is extremely verbose. In JSON and similar, the emphasis is on the description of each field of an object rather than describing the object as a whole. Modeling a heterogeneous structure requires an auxiliary field which does not actually hold data, but rather serves as a discriminator. In CHT, the focus is instead on the type of each node/object. On the other hand the fields are not mentioned at all - instead the encoded values are mapped based on their order and the type of the node.

## What is this for
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
            "Variable": {"Symbol": "x"},
            "Value": {
                "Kind": "List",
                "Children": [
                    {"Kind": "Number", "Value": 0},
                    {"Kind": "Number", "Value": 58},
                    {"Kind": "Number", "Value": 15},
                ]
            }
        },
        {
            "Kind": "MethodCall",
            "Object": {"Symbol": "x"},
            "Method": {"Symbol": "append"},
            "Arguments": [{"Kind": "Number", "Value": 7}]
        },
        {
            "Kind": "Assignment",
            "Variable": {"Symbol": "y"},
            "Value": {
                "Kind": "Indexing",
                "Collection": {"Symbol": "x"},
                "Index": {"Kind": "Number", "Value": 3}
            }
        },
        {
            "Kind": "FunctionCall",
            "Function": {"Symbol": "print"},
            "Arguments": [{"Symbol": "y"}]
        },
        {
            "Kind": "Assignment",
            "Variable": {"Symbol": "text"},
            "Value": {"Kind": "Text", "Value": "Some text with spaces"}
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
CHT is an abstract format. A representation of a CHT file is up to the communicating parties to choose. What this means, is that there are no data types like arrays, integers or strings in CHT. Instead CHT only describes a tree structure of data.
There are three kinds of nodes:

1. Terminal node - a leaf node. This node is defined by the values of its raw and quoted parts. Often either purely raw or purely quoted, but both parts may be present.
3. Nonterminal node - a node which may have children. Such node is entirely defined by its type (capital letter followed by any number of nonspecial characters) and a sequence (potentially empty) of its children.


### Syntax
CHT is whitespace sensitive. The entire document can be a single line, using parentheses to denote nesting, but when human readibility is a concern, nesting can instead be encoded using indentation.

#### Indentation
CHT is an indentation sensitive format. The indentation of each line is the sequence of white space characters up to the first nonwhitespace. The indentation of each line must either stay the same (be exactly the same as the previous line), increase (the previous line is a prefix of the current line), or decrease (be a prefix of a previous line which has occured before). The first line must start at a zero indentation.

#### Special characters
Whitespace characters including a space ` `, tab, newline and others, a colon `:`, a quotation mark `"` and parentheses `()` have special meanings in CHT. Other characters can be used freely.

#### Terminal node
A concatenation of a raw and quoted parts. One of the parts may be missing. The raw part is a piece of text that does not start with a capital letter and does not contain a special character. Colons `:` can be present as long as they have digits on both sides.

The quoted part corresponds exactly with a JSON string syntax. That is the node's value is delimited using a pair of `"` and may contain escape sequences using `\`.

An example of purely raw terminal: `$print`, a purely quoted terminal: `"Some text"` and combined terminal: `re"[A-Z]\w*"`.

#### Nonterminal node
There are three ways to spell out a nonterminal node. Note that nodes with zero children can only use the function syntax.

##### 1. Rest of line syntax
A type of the node followed by a colon `:` and a non-newline whitespace separated non-empty sequence of either one of terminal nodes, a rest of line syntax nonterminal nodes or a function syntax nonterminal nodes representing the nodes children. Extra whitespace at the begining, end or in between of the children is ignored.

For example:

```
RestOfLineNonterminal: "its first child" "its second child"
```


##### 2. Function syntax
A type of the node followed by a left paren `(`a non-newline whitespace separated sequence of either one of terminal nodes, a rest of line syntax nonterminal nodes or a function syntax nonterminal nodes representing the nodes children and terminated with a right paren `)` Extra whitespace at the begining, end or in between of the children is ignored.

For example:

```
FunctionNonterminal("its first child" "its second child")
```

Note that this syntax cannot span multiple lines.

##### 3. Block syntax
A type of the node followed by a colon `:` a newline an indentation increase and a newline separated non-empty sequence of any nodes and terminated with an indentation decrease.

For example:

```
BlockNonterminal:
    "its first child"
    "its second child"
```

#### Root
The root may be any node as defined above.

## Implementations

CHT has libraries in the following languages:

- [C#](cs)
