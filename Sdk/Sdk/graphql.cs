namespace Stratumn.Sdk
{
    public static class GraphQL
    {
        public const string FRAGMENT_PAGINATIONINFO_ONLINKSCONNECTION =
            @"fragment PaginationInfoOnLinksConnectionFragment on LinksConnection {
                totalCount
                info: pageInfo {
                    hasNext: hasNextPage
                    hasPrevious: hasPreviousPage
                    startCursor
                    endCursor
                }
            }";

        public const string FRAGMENT_PAGINATIONINFO_ONTRACESCONNECTION =
            @"fragment PaginationInfoOnTracesConnectionFragment on TracesConnection {
                totalCount
                info: pageInfo {
                    hasNext: hasNextPage
                    hasPrevious: hasPreviousPage
                    startCursor
                    endCursor
                }
            }";

        public const string FRAGMENT_TRACESTATE =
            @"fragment TraceStateFragment on Trace {
                updatedAt
                state {
                    data
                }
                head {
                    raw
                    data
                    groupId
                }
                # TODO: temporary, remove once state computation
                # is handled server side
                links {
                    nodes {
                        raw
                        data
                    }
                }
            }";

        public const string FRAGMENT_HEADLINK =
            @"fragment HeadLinkFragment on Trace {
                head {
                    raw
                    data
                }
            }";

        public const string MUTATION_CREATELINK =
            @"mutation createLinkMutation($link: JSON!, $data: JSON) {
                createLink(input: { link: $link, data: $data }) {
                    trace {
                        ...TraceStateFragment
                    }
                }
            }" + FRAGMENT_TRACESTATE;

        public const string MUTATION_ADDTAGSTOTRACE =
            @"mutation addTagsToTraceMutation($traceId: UUID!, $tags: [String]!) {
                addTagsToTrace(input: { traceRowId: $traceId, tags: $tags }) {
                    trace {
                        ...TraceStateFragment
                    }
                }
            }" + FRAGMENT_TRACESTATE;

        public const string QUERY_CONFIG =
            @"query ConfigQuery($workflowId: BigInt!) {
                account: myAccount {
                    accountId: rowId
                    signingKey {
                        privateKey {
                            passwordProtected
                            decrypted
                        }
                    }
                    bot {
                        teams {
                            nodes {
                                accountId
                            }
                        }
                    }
                    user {
                        memberOf {
                            nodes {
                                accountId: rowId
                            }
                        }
                    }
                }
                workflow: workflowByRowId(rowId: $workflowId) {
                    config { id: rowId }
                    groups {
                        nodes {
                            groupId: rowId
                            label
                            members {
                                nodes {
                                    accountId
                                }
                            }
                        }
                    }
                }
            }";

        public const string QUERY_GETHEADLINK =
            @"query getHeadLinkQuery($traceId: UUID!) {
                trace: traceById(id: $traceId) {
                    ...HeadLinkFragment
                }
            }" + FRAGMENT_HEADLINK;

        public const string QUERY_GETTRACEDETAILS =
            @"query getTraceDetailsQuery(
                $traceId: UUID!
                $first: Int
                $last: Int
                $before: Cursor
                $after: Cursor
            ) {
                trace: traceById(id: $traceId) {
                    links(first: $first, last: $last, before: $before, after: $after) {
                        nodes {
                            raw
                            data
                        }
                        ...PaginationInfoOnLinksConnectionFragment
                    }
                }
            }" + FRAGMENT_PAGINATIONINFO_ONLINKSCONNECTION;

        public const string QUERY_GETTRACESINSTAGE =
            @"query getTracesInStageQuery(
                $groupId: BigInt!
                $stageType: StageType!
                $actionKey: String
                $first: Int
                $last: Int
                $before: Cursor
                $after: Cursor
            ) {
                group: groupByRowId(rowId: $groupId) {
                    stages(condition: { type: $stageType, actionKey: $actionKey }) {
                        nodes {
                            traces(first: $first, last: $last, before: $before, after: $after) {
                                nodes {
                                    ...TraceStateFragment
                                }
                                ...PaginationInfoOnTracesConnectionFragment
                            }
                        }
                    }
                }
            }" + FRAGMENT_TRACESTATE + FRAGMENT_PAGINATIONINFO_ONTRACESCONNECTION;

        public const string QUERY_GETTRACESTATE =
            @"query getTraceStateQuery($traceId: UUID!) {
                trace: traceById(id: $traceId) {
                    ...TraceStateFragment
                }
            }" + FRAGMENT_TRACESTATE;

        public const string QUERY_SEARCHTRACES =
            @"query searchTracesQuery(
                $workflowId: BigInt!
                $first: Int
                $last: Int
                $before: Cursor
                $after: Cursor
                $filter: TraceFilter!
            ) {
                workflow: workflowByRowId(rowId: $workflowId) {
                    traces(
                        first: $first
                        last: $last
                        before: $before
                        after: $after
                        filter: $filter
                    ) {
                        nodes {
                            ...TraceStateFragment
                        }
                        ...PaginationInfoOnTracesConnectionFragment
                    }
                }
            }" + FRAGMENT_TRACESTATE + FRAGMENT_PAGINATIONINFO_ONTRACESCONNECTION;
    }
}
