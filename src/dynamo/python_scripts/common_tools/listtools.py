# List flattening function. To flatten max nesting of 5 levels.
def list_flatten(digits):
    """Flatten nested lists. Up to 5 levels."""
    flattened_list = []
    for l in digits:
        if isinstance (l,list):
            for ll in l:
                if isinstance (ll,list):
                    for lll in ll:
                        if isinstance (lll,list):
                            for llll in lll:
                                if isinstance (llll,list):
                                    for lllll in llll:
                                        flattened_list.append(lllll)
                                else:
                                    flattened_list.append(llll)
                        else:
                            flattened_list.append(lll)
                else:
                    flattened_list.append(ll)
        else:
            flattened_list.append(l)
    return flattened_list

# Sequence function. To generate a sequence of numbers.
def sequence(start,number,step):
    """Produce a sequence of numbers."""
    number_list = []
    a = 0
    b = start - step
    while a < number:
        a = a+1
        b = b+step
        number_list.append(b)
    return number_list
