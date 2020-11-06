# Dimensionalize

## Summary
Convert a CSV file from multiple values per row, into single dimensionalized value per row.

The conversion is performed using column heading pattern matching.

```
Usage: dimensionalize <filename> [options] <col-spec> [col-spec]...[col-spec]
        <col-spec>                  Defines the how to map a column in the source data file to dimensions.
        --sanitize                  Tidy up values stripping out quotes and commas.
        --quote-labels              Put quotations around all text labels.
        --ignore-unknown-columns    When a column does not map correctly, do not produce any output.
        --new-headings=custom1,custom2...etc.   New column headings for first row of output.
```

## Compiling and running
1. `dotnet build`
2. `dotnet run bin/debug/dimensionalize.dll`

## Example
Consider a CSV file (test.csv) contains data columns of: Boys,BoysGCSE,Girls,GirlsGCSE,AllGCSE,All

dimensionalize test.csv --sanitize "Boys,Girls,All" "GCSE,"

Notice how in the case where there is no GCSE there is just an "All", to handle this where we put a
comma and no label, the match is an empty string.

To rename value labels when mapped to dimensions, assign as follows:

dimensionalize test.csv --sanitize "Boys=Male,Girls=Female,All" "GCSE,=No GCSE
This would remap the dimensionalized labels from Boys to Male etc. and where there is no label
in the case of no GCSE, the label "No GCSE" will be used.