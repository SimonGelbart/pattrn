# Research summary

Research branches evaluated Pattrn against a Homework subscription-routing
workload, grouped structural registrations, a specialized lean matcher, and
compiler scaling and mutation trade-offs.

The compiler correction was valuable: structural hashing removes accidental
quadratic duplicate analysis. Grouping values by structural pattern can also
materially reduce registrations in workloads with shared patterns. The
specialized lean matcher was not compelling enough to maintain as product code.

Those experiments remain available on their research branches and are not part
of the supported product surface.
