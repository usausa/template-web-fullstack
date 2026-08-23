SELECT * FROM Data
WHERE (/*@ name */'' IS NULL) OR (Name LIKE '%' || /*@ name */'' || '%')
ORDER BY
/*% if (sort == "Name") { */
Name
/*% } else if (sort == "Value") { */
Value
/*% } else if (sort == "CreatedAt") { */
CreatedAt
/*% } else { */
Id
/*% } */
/*% if (desc) { */
DESC
/*% } */
LIMIT /*@ size */10 OFFSET /*@ offset */0
