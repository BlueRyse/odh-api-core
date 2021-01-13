﻿namespace RawQueryParser

/// <summary>
/// Differentiate between an identifier segment, e.g. 'Detail' and an array segment: []
/// </summary>
type FieldSegment =
    | IdentifierSegment of string
    | ArraySegment

/// <summary>
/// Defines a field with hierarchial access fields.
/// E.g. <c>Field [IdentifierSegment "Detail"; IdentifierSegment "de"; IdentifierSegment "Title"]</c>
/// or <c>Field [IdentifierSegment "Features"; ArraySegment; IdentifierSegment "Id"]</c>
/// </summary>
type Field = Field of fields: FieldSegment list

module Sorting =
    /// Defines the sort direction.
    type OrderBy =
        | Ascending
        | Descending

    /// A sort statement is composed of a
    /// property and a sort direction.
    type SortStatement =
        { Field: Field
          Direction: OrderBy }

    /// Sort statements are a list of sort statement.
    type SortStatements = SortStatement list

module Filtering =
    /// Defines the possible operators.
    type Operator =
        | Eq
        | Ne
        | Lt
        | Le
        | Gt
        | Ge

    /// Defines the values of tye booleans, number or string.
    type Value =
        | Boolean of bool
        | Number of float
        | String of string

    /// The condition is the combination of a property, an operator and a value.
    type Comparison =
        { Field: Field
          Operator: Operator
          Value: Value }

    type Condition =
        | Comparison of Comparison
        | In of field: Field * values: Value list
        | NotIn of field: Field * values: Value list
        | IsNull of Field
        | IsNotNull of Field

    /// A filter statement can be a simple condition or a bunch
    /// of conditions inside an AND or an OR binary statement.
    type FilterStatement =
        | And of left: FilterStatement * right: FilterStatement
        | Or of left: FilterStatement * right: FilterStatement
        | Condition of Condition

